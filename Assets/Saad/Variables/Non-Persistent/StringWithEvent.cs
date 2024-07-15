using UnityEngine;
using ProjectCore.Variables;
using ProjectCore.Events;

public class StringWithEvent : SharedString
{
    [SerializeField] protected GameEvent GameEvent;
    public override void SetValue(string value)
    {
        base.SetValue(value);
        GameEvent?.Invoke();
    }
}
