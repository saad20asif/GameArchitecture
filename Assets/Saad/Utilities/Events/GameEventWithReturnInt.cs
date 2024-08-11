using ProjectCore.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectCore.Events
{
    [CreateAssetMenu(fileName = "e_", menuName = "ProjectCore/Events/GameEvent - ReturnInt")]
    public class GameEventWithReturnInt : GameEventWithReturn<int>
    {
        [Button]
        [GUIColor(1, 1, 0.5f)]
        public override int Raise()
        {
            return base.Raise();
        }
    }
}
