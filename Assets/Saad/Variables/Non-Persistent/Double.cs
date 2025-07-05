using UnityEngine;

namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Non-Persistent/Double")]
    public class Double : ScriptableObject
    {
        [SerializeField] protected double Value;
        [SerializeField] protected double DefaultValue;
        [SerializeField] protected bool ResetToDefaultOnPlay;

        private void OnEnable()
        {
            if (ResetToDefaultOnPlay)
                Value = DefaultValue;
        }
        public virtual void SetValue(double value)
        { Value = value; }
        public virtual double GetValue() { return Value; }
        public virtual void SetDefaultValue(double value)
        { DefaultValue = value; }
        
        public virtual double GetDefaultValue() 
        { return DefaultValue; }

    }
}
