using Blues.Core.Events;
using UnityEngine;

namespace Blues.Core.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Non-Persistent/FloatWithEvent")]
    public class FloatWithEvent : Float
    {
        [SerializeField] protected GameEvent GameEvent;
        public override void Decrement(float _decrement)
        {
            base.Decrement(_decrement);
            GameEvent?.Invoke();
        }
        public override void Increment(float _increment)
        {
            base.Increment(_increment);
            GameEvent?.Invoke();
        }

        public override void SetValue(float value)
        {
            base.SetValue(value);
            GameEvent?.Invoke();
        }
    }
}