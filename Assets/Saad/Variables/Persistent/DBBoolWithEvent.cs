using UnityEngine;
using ProjectCore.Events;

[CreateAssetMenu(fileName = "vDBBoolWithEvent_", menuName = "ProjectCore/Variables/Persistent/DBBoolWithEvent")]
public class DBBoolWithEvent : DBBool
{
    [SerializeField] protected GameEvent GameEvent;
    public override void SetValue(bool value)
    {
        base.SetValue(value);
        GameEvent.Invoke();
    }
}
