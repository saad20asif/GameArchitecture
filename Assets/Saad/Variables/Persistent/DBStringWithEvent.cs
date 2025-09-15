using ProjectCore.Events;
using UnityEngine;

namespace ProjectCore.Variables
{
    [CreateAssetMenu(fileName = "v_", menuName = "ProjectCore/Variables/Persistent/DBStringWithEvent")]
    public class DBStringWithEvent : DBString
    {
        [SerializeField] protected GameEvent GameEvent;
        public override void SetValue(string value)
        {
            base.SetValue(value);
            GameEvent.Invoke();
        }
    }
}

