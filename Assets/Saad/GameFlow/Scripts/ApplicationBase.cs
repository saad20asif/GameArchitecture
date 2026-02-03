using UnityEngine;
using System.Collections;
using Blues.Core.StateMachine;
using Blues.Core.TheTimeMachine;
using Blues.Core.Variables;


namespace Blues.Core.Application
{
    public class ApplicationBase : MonoBehaviour
    {
        [SerializeField] private int AndroidFrameRate = 60;
        [SerializeField] private int IOSFrameRate = 60;
        [SerializeField] private Float SceneLoadingProgress;
        [SerializeField] private FiniteStateMachine FiniteStateMachine;
        [SerializeField] private TimeMachine TimeMachine;
        
        private Coroutine _timeMachineCo;

        private IEnumerator Start()
        {
            UnityEngine.Application.targetFrameRate = AndroidFrameRate;
            _timeMachineCo = StartCoroutine(TimeMachine.Tick());
            yield return FiniteStateMachine.Init();
        }
    }
}