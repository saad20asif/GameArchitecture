using UnityEngine;

namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "vSharedVector2Int_", menuName = "ProjectCore/Variables/Simple/SharedVector2Int")]
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
