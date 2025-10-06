using UnityEngine;
using System.Collections;
using Blues.Core.Events;

namespace Blues.Core.TheTimeMachine
{
    [CreateAssetMenu(fileName = "TimeMachine", menuName = "ProjectCore/TimeMachine")]
    public class TimeMachine : ScriptableObject
    {
        [SerializeField] private GameEvent _tick;
        private readonly WaitForSeconds _waitForOneSecond = new WaitForSeconds(1);
        public IEnumerator Tick()
        {
            while (true)
            {
                _tick.Invoke();
                yield return _waitForOneSecond;
            }
        }
    }
}
