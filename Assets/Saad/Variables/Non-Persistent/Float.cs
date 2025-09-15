using UnityEngine;

namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Non-Persistent/Float")]
    public class Float : ScriptableObject
    {
        [SerializeField] protected float Value;
        [SerializeField] protected float DefaultValue;
        [SerializeField] protected bool ResetToDefaultOnPlay;

        private void OnEnable()
        {
            if (ResetToDefaultOnPlay)
                Value = DefaultValue;

        }
        public virtual void SetValue(float value)
        { Value = value; }
        
        public virtual void SetDefaultValue(float value)
        { DefaultValue = value; }
        
        public virtual float GetDefaultValue() 
		{ return DefaultValue; }

        public virtual float GetValue() { return Value; }
        
        public virtual void Increment(float _increment)
        { Value += _increment; }

        public virtual void Decrement(float _decrement)
        { Value -= _decrement; }
    }
}
