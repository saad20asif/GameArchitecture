using UnityEngine;

namespace Blues.Core.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Non-Persistent/SharedVector2Int")]
    public class SharedVector2Int : ScriptableObject
    {
        [SerializeField] protected Vector2Int Value;
        [SerializeField] protected Vector2Int DefaultValue;
        [SerializeField] protected bool ResetToDefaultOnPlay;

        private void OnEnable()
        {
            if (ResetToDefaultOnPlay)
                Value = DefaultValue;

        }
        public virtual void SetValue(Vector2Int value)
        { Value = value; }
        public virtual Vector2Int GetValue() { return Value; }
    }
}
