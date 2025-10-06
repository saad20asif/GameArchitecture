using UnityEngine;

namespace Blues.Core.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Non-Persistent/Bool")]
    public class Bool : ScriptableObject
    {
        [SerializeField] protected bool Value;
        [SerializeField] protected bool DefaultValue;
        [SerializeField] protected bool ResetToDefaultOnPlay;

        private void OnEnable()
        {
            if (ResetToDefaultOnPlay)
                Value = DefaultValue;

        }
        public virtual void SetValue(bool value)
        { Value = value; }
        public virtual bool GetValue() { return Value; }
        
        public virtual void SetDefaultValue(bool value)
        { DefaultValue = value; }
        
        public virtual bool GetDefaultValue() 
        { return DefaultValue; }
    }
}
