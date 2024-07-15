using ProjectCore.Events;
using ProjectCore.Variables;
using UnityEngine;

namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "vIntWithEvent_", menuName = "ProjectCore/Variables/Simple/IntWithEvent")]
    public class IntWithEvent : Int
    {
        [SerializeField] protected GameEvent gameEvent;
        public override void Decrement(int _decrement)
        {
            base.Decrement(_decrement);
            gameEvent.Invoke();
        }
        public override void Increment(int _increment)
        {
            base.Increment(_increment);
            gameEvent.Invoke();
        }

        public override void SetValue(int value)
        {
            base.SetValue(value);
            gameEvent.Invoke();
        }
    }
}
