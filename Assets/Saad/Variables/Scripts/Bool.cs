using UnityEngine;

[CreateAssetMenu(fileName = "vBool_", menuName = "ProjectCore/Variables/Simple/Bool")]
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
}
