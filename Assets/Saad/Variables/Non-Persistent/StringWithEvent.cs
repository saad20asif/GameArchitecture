using UnityEngine;
using ProjectCore.Events;

namespace ProjectCore.Variables
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

