using UnityEngine;
using ProjectCore.StateMachine;
using System.Collections;
using ProjectCore.Variables;
using ProjectCore.TheTimeMachine;


namespace ProjectCore.Application
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
            Debug.Log("ApplicationBase Start called");
            _timeMachineCo = StartCoroutine(TimeMachine.Tick());
            yield return FiniteStateMachine.Init();
        }
    }
}