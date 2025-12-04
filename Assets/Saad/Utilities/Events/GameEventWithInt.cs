using UnityEngine;
using Sirenix.OdinInspector;
namespace Blues.Core.Events
{
    [CreateAssetMenu(fileName = "e_", menuName = "ProjectCore/Events/GameEvent - Int")]
    public class GameEventWithInt : GameEventWithParam<int>
    {
        [Button]
        [GUIColor(1, 1, 0.5f)]
        public override void Raise(int t)
        {
            base.Raise(t);
        }
    }
}
