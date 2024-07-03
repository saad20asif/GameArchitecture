using UnityEngine;

[CreateAssetMenu(fileName = "vSharedVector2_", menuName = "ProjectCore/Variables/Simple/SharedVector2")]
public class SharedVector2 : ScriptableObject
{
    [SerializeField] protected Vector2 Value;
    [SerializeField] protected Vector2 DefaultValue;
    [SerializeField] protected bool ResetToDefaultOnPlay;

    private void OnEnable()
    {
        if (ResetToDefaultOnPlay)
            Value = DefaultValue;

    }
    public virtual void SetValue(Vector2 value)
    { Value = value; }
    public virtual Vector2 GetValue() { return Value; }
}
