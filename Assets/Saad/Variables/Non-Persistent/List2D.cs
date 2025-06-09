using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace ProjectCore.Variables
{
    public abstract class List2D<T> : SerializedScriptableObject
    {
        [SerializeField] protected List<List<T>> values = new List<List<T>>();
        [SerializeField] protected List<List<T>> defaultValue = new List<List<T>>();
        [SerializeField] protected bool resetToDefaultOnPlay = true;

        protected virtual void OnEnable()
        {
            if (resetToDefaultOnPlay)
            {
                ResetToDefault();
            }
        }

        public virtual void ResetToDefault()
        {
            values = new List<List<T>>();
            foreach (var innerList in defaultValue)
            {
                values.Add(new List<T>(innerList));
            }
        }

        public virtual T GetValue(int row, int column)
        {
            if (row < 0 || row >= values.Count)
            {
                Debug.LogError($"Row index {row} out of range!");
                return default;
            }

            if (column < 0 || column >= values[row].Count)
            {
                Debug.LogError($"Column index {column} out of range in row {row}!");
                return default;
            }

            return values[row][column];
        }

        public virtual void SetValue(int row, int column, T value)
        {
            if (row < 0 || row >= values.Count)
            {
                Debug.LogError($"Row index {row} out of range!");
                return;
            }

            if (column < 0 || column >= values[row].Count)
            {
                Debug.LogError($"Column index {column} out of range in row {row}!");
                return;
            }

            values[row][column] = value;
        }

        public virtual void AddRow()
        {
            values.Add(new List<T>());
        }

        public virtual void AddElement(int row, T element)
        {
            if (row < 0 || row >= values.Count)
            {
                Debug.LogError($"Row index {row} out of range!");
                return;
            }

            values[row].Add(element);
        }

        public virtual void RemoveRow(int row)
        {
            if (row < 0 || row >= values.Count)
            {
                Debug.LogError($"Row index {row} out of range!");
                return;
            }

            values.RemoveAt(row);
        }

        public virtual void RemoveElement(int row, int column)
        {
            if (row < 0 || row >= values.Count)
            {
                Debug.LogError($"Row index {row} out of range!");
                return;
            }

            if (column < 0 || column >= values[row].Count)
            {
                Debug.LogError($"Column index {column} out of range in row {row}!");
                return;
            }

            values[row].RemoveAt(column);
        }

        public virtual int RowCount => values.Count;

        public virtual int ColumnCount(int row)
        {
            if (row < 0 || row >= values.Count) return 0;
            return values[row].Count;
        }
    }

    // Example implementation for int
    [CreateAssetMenu(fileName = "List2DInt_", menuName = "ProjectCore/Variables/Non-Persistent/List2DInt")]
    public class List2DInt : List2D<int> { }

    // Example implementation for float
    [CreateAssetMenu(fileName = "List2DFloat_", menuName = "ProjectCore/Variables/Non-Persistent/List2DFloat")]
    public class List2DFloat : List2D<float> { }
}