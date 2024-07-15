using ProjectCore.Events;
using ProjectCore.Variables;
using UnityEngine;

namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "vIntWithEvent_", menuName = "ProjectCore/Variables/Simple/IntWithEvent")]
    public class IntWithEvent : Int
    {
        [SerializeField] protected GameEvent GameEvent;
        public override void Decrement(int _decrement)
        {
            base.Decrement(_decrement);
            GameEvent.Invoke();
        }
        public override void Increment(int _increment)
        {
            base.Increment(_increment);
            GameEvent.Invoke();
        }

        public override void SetValue(int value)
        {
            base.SetValue(value);
            GameEvent.Invoke();
        }
    }
}
