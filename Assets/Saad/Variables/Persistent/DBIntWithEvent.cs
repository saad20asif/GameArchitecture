using Blues.Core.Events;
using UnityEngine;

namespace Blues.Core.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Persistent/DBIntWithEvent")]
    public class DBIntWithEvent : DBInt
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

      
