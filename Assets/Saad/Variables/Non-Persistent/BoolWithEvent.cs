using Blues.Core.Events;
using UnityEngine;

namespace Blues.Core.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Non-Persistent/BoolWithEvent")]
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
