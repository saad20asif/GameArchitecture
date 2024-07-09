using UnityEngine;
using ProjectCore.StateMachine;
using System.Collections;
using ProjectCore.Variables;


namespace ProjectCore.Application
{
    public class ApplicationBase : MonoBehaviour
    {
        [SerializeField] private int AndroidFrameRate = 60;
        [SerializeField] private int IOSFrameRate = 60;
        [SerializeField] private Float SceneLoadingProgress;
        [SerializeField] private FiniteStateMachine FiniteStateMachine;

        private IEnumerator Start()
        {
            UnityEngine.Application.targetFrameRate = AndroidFrameRate;
            Debug.Log("ApplicationBase Start called");
            yield return FiniteStateMachine.Init();

        }

    }
}