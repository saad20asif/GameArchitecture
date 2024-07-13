using UnityEngine;
using ProjectCore.Events;
using System.Collections;

namespace ProjectCore.TheTimeMachine
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
                //Debug.Log("Ticking");
                yield return _waitForOneSecond;
                _tick.Invoke();
            }
        }
    }
}
