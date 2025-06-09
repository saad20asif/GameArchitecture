using UnityEngine;

namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "vSharedString_", menuName = "ProjectCore/Variables/Non-Persistent/SharedString")]
    public class SharedString : ScriptableObject
    {
        [SerializeField] protected string Value;
        [SerializeField] protected string DefaultValue;
        [SerializeField] protected bool ResetToDefaultOnPlay;

        private void OnEnable()
        {
            if (ResetToDefaultOnPlay)
                Value = DefaultValue;

        }
        public virtual void SetValue(string value)
        { Value = value; }
        public virtual string GetValue() { return Value; }
    }
}
