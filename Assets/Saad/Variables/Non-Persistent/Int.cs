using UnityEngine;

namespace Blues.Core.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Non-Persistent/Int")]
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
        
        public virtual void SetDefaultValue(int value)
        { DefaultValue = value; }

        public virtual void Increment(int _increment)
        { Value += _increment; }

        public virtual void Decrement(int _decrement)
        { Value -= _decrement; }
        public virtual int GetValue() { return Value; }
    }
}
