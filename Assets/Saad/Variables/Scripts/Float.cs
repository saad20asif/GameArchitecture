using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "vFloat_", menuName = "ProjectCore/Variables/Float")]
public class Float : ScriptableObject
{
    [SerializeField] protected float Value;
    [SerializeField] protected float DefaultValue;
    [SerializeField] protected bool ResetToDefaultOnPlay;

    private void OnEnable()
    {
        if (ResetToDefaultOnPlay)
            Value = DefaultValue;

    }
    public virtual void SetValue(float value)
    { Value = value; }
    public virtual float GetValue() { return Value; }
}
