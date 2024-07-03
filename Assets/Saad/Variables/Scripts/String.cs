using UnityEngine;

[CreateAssetMenu(fileName = "vString_", menuName = "ProjectCore/Variables/String")]
public class String : ScriptableObject
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
