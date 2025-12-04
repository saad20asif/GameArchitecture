using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using System.IO;
using Blues.Core.Variables;

namespace Featrues.SpinWheel
{
    [CreateAssetMenu(fileName = "SpinWheel", menuName = "Scriptable Objects/SpinWheelConfigs")]
    public class SpinWheelConfigurations : ScriptableObject
    {
        [SerializeField] private SharedString FilePath;
        private string _filePath ;
        [SerializeField] private SpinWheelData spinWheelData;
        public SpinWheelData loadedSpinWheelData;


        [Header("Slice UI")]
        public Sprite sliceSprite;
        public Sprite coinSprite;
        
        [Header("Spin Settings")]
        public int totalIterations;
        public float spinTime;
        public AnimationCurve SpinningCurve;

        [Header("Test Settings")]
        public bool isTesting; 
        public int stopIndex;

        public void OnEnable()
        {
            _filePath =  Path.Combine(Application.persistentDataPath, FilePath.GetValue());
        }
        [Button(buttonSize: 2, ButtonAlignment = 1)]
        public void SaveDataToJson()
        {
            if(spinWheelData != null)
            {
                SpinWheelJsonFormat spinwheelJsonData = new SpinWheelJsonFormat(spinWheelData);
                string json = JsonConvert.SerializeObject(spinwheelJsonData, Formatting.Indented);
                File.WriteAllText(_filePath, json);
                Debug.Log("Data written to JSON. at path " + _filePath);
            }
            else
            {
                Debug.LogError("spinWheelData not found");
            }
        
        }

        public bool LoadDataFromJson()
        {
            if (!File.Exists(_filePath))
            {
                Debug.LogError("Save file not found at: " + _filePath);
                return false;
            }

            string json = File.ReadAllText(_filePath);
            Debug.Log(json);
            SpinWheelJsonFormat loadedJsonData = JsonConvert.DeserializeObject<SpinWheelJsonFormat>(json);
            loadedJsonData.DebugData();
            if (loadedJsonData != null)
            {
                loadedSpinWheelData.ConvertToSpinWheelData( loadedJsonData); 
                
                return true;
            }
            else
            {
                Debug.LogError("Failed to deserialize SpinWheel data.");
                return false;
            }
        }


    }
}

