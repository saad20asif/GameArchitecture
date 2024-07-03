using UnityEngine;

[CreateAssetMenu(fileName = "vSharedVector3_", menuName = "ProjectCore/Variables/Simple/SharedVector3")]
public class SharedVector3 : ScriptableObject
{
    [SerializeField] protected Vector3 Value;
    [SerializeField] protected Vector3 DefaultValue;
    [SerializeField] protected bool ResetToDefaultOnPlay;

    private void OnEnable()
    {
        if (ResetToDefaultOnPlay)
            Value = DefaultValue;

    }
    public virtual void SetValue(Vector3 value)
    { Value = value; }
    public virtual Vector3 GetValue() { return Value; }
}
