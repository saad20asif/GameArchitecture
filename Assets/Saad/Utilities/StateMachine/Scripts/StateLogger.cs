using Blues.Core.StateMachine;
using UnityEngine;

public class StateLogger : MonoBehaviour
{
    [SerializeField] private FiniteStateMachine fsm;

    private void OnEnable()
    {
        fsm.OnStateEntered += HandleEntered;
        fsm.OnStateExited += HandleExited;
        fsm.OnStatePaused += HandlePaused;
        fsm.OnStateResumed += HandleResumed;
    }

    private void OnDisable()
    {
        fsm.OnStateEntered -= HandleEntered;
        fsm.OnStateExited -= HandleExited;
        fsm.OnStatePaused -= HandlePaused;
        fsm.OnStateResumed -= HandleResumed;
    }

    private void HandleEntered(State state) => Debug.Log($"ENTERED::::: {state.name}");
    private void HandleExited(State state) => Debug.Log($"EXITED::::: {state.name}");
    private void HandlePaused(State state) => Debug.Log($"PAUSED::::: {state.name}");
    private void HandleResumed(State state) => Debug.Log($"RESUMED::::: {state.name}");
}

