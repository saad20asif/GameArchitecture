using UnityEngine;

namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "vInt_", menuName = "ProjectCore/Variables/Non-Persistent/Int")]
    public class Int : ScriptableObject
    {
        [SerializeField] protected int Value;
        [SerializeField] protected int DefaultValue;
        [SerializeField] protected bool ResetToDefaultOnPlay;

        private void OnEnable()
        {
            if (ResetToDefaultOnPlay)
                Value = DefaultValue;

        }
        public virtual void SetValue(int value)
        { Value = value; }

        public virtual void Increment(int _increment)
        { Value += _increment; }

        public virtual void Decrement(int _decrement)
        { Value -= _decrement; }
        public virtual float GetValue() { return Value; }
    }
}
