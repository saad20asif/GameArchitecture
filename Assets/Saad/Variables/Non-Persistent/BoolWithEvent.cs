using UnityEngine;
using ProjectCore.Events;
namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "vBoolWithEvent_", menuName = "ProjectCore/Variables/Simple/BoolWithEvent")]
    public class BoolWithEvent : Bool
    {
        [SerializeField] protected GameEvent GameEvent;
        public override void SetValue(bool value)
        {
            base.SetValue(value);
            GameEvent.Invoke();
        }
    }
}
