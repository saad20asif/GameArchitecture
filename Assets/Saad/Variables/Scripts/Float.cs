using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Float")]
public class Float : ScriptableObject
{
    [SerializeField] protected float Value;
    [SerializeField] protected float DefaultValue;
    [SerializeField] protected bool ResetToDefault;

    private void OnEnable()
    {
        if (ResetToDefault)
            Value = DefaultValue;

    }
    public void SetValue(float value)
    { Value = value; }
    public float GetValue() { return Value; }
}
