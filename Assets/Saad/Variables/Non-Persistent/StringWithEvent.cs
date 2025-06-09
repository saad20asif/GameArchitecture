using UnityEngine;
using ProjectCore.Variables;
using ProjectCore.Events;

[CreateAssetMenu(fileName = "vStringWithEvent_", menuName = "ProjectCore/Variables/Non-Persistent/StringWithEvent")]
public class StringWithEvent : SharedString
{
    [SerializeField] protected GameEvent GameEvent;
    public override void SetValue(string value)
    {
        base.SetValue(value);
        GameEvent?.Invoke();
    }
}
