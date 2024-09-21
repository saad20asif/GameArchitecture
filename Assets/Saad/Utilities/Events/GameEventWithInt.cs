using UnityEngine;
using Sirenix.OdinInspector;
namespace ProjectCore.Events
{
    [CreateAssetMenu(fileName = "e_", menuName = "ProjectCore/Events/GameEvent - Int")]
    public class GameEventWithInt : GameEventWithParam<int>
    {
        [Button]
        [GUIColor(1, 1, 0.5f)]
        protected override void Raise(int t)
        {
            base.Raise(t);
        }
    }
}
