using UnityEngine;

[CreateAssetMenu(fileName = "vDouble_", menuName = "ProjectCore/Variables/Simple/Double")]
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
}
