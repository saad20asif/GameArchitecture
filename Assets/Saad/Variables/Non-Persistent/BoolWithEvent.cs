using UnityEngine;
using ProjectCore.Events;
namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "vBoolWithEvent_", menuName = "ProjectCore/Variables/Non-Persistent/BoolWithEvent")]
    public class BoolWithEvent : Bool
    {
        [SerializeField] protected GameEvent GameEvent;
        public override void SetValue(bool value)
        {
            base.SetValue(value);
            GameEvent?.Invoke();
        }
    }
}
