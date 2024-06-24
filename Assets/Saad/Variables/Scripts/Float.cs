using Sirenix.OdinInspector;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Float")]
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
    public void SetValue(float value)
    { Value = value; }
    public float GetValue() { return Value; }
}
