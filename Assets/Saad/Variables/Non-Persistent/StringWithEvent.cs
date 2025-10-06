using Blues.Core.Events;
using UnityEngine;

namespace Blues.Core.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Non-Persistent/StringWithEvent")]
    public class StringWithEvent : SharedString
    {
        [SerializeField] protected GameEvent GameEvent;
        public override void SetValue(string value)
        {
            base.SetValue(value);
            GameEvent?.Invoke();
        }
    }
}

