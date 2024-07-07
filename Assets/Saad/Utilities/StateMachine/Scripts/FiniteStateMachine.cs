using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "FiniteStateMachine", menuName = "ProjectCore/State Machine/Basic FSM")]
public class FiniteStateMachine : SerializedScriptableObject, IState
{
    [SerializeField] private State BootState; // The initial state of the FSM
    [SerializeField] private State CurrentState; // The current active state
    [SerializeField] private Stack<State> PausedStates = new Stack<State>(); // Stack to manage paused states

    // Initializes the FiniteStateMachine with the BootState
    public IEnumerator Init()
    {
        Debug.Log("FiniteStateMachine Init called");

        if (BootState == null)
        {
            Debug.LogWarning("Please assign a boot state in FiniteStateMachine to start the game!");
            yield break;
        }

        CurrentState = BootState;
        yield return CurrentState.Enter(this);
        PausedStates.Clear(); // Clear paused states stack
    }

    // Implements the TransitionTo method from the IState interface
    public void TransitionTo(Transition transition)
    {
        if (transition == null || transition.ToState == null)
        {
            Debug.LogWarning("Invalid transition or target state.");
            return;
        }

        CoroutineRunner.instance.StartCoroutine(DoTransition(transition));
    }

    // Performs the state transition
    private IEnumerator DoTransition(Transition transition)
    {
        Debug.Log("DoTransition " + transition.ToState.name);

        if (CurrentState != null)
        {
            State nextState = transition.ToState;

            if (nextState.PausePreviousState)
            {
                // Pause the current state and push it onto the stack
                Debug.Log("Pausing current state.");
                yield return CurrentState.Pause();
                PausedStates.Push(CurrentState);
            }
            else
            {
                // If the next state is not found in the paused stack, clear the stack
                if (!IsStateInPausedStack(nextState))
                {
                    Debug.Log("Clearing all paused states.");
                    while (PausedStates.Count > 0)
                    {
                        State pausedState = PausedStates.Pop();
                        yield return pausedState.Exit();
                    }
                }

                // Exit the current state
                yield return CurrentState.Exit();
            }

            CurrentState = nextState;

            if (IsStateInPausedStack(nextState))
            {
                // Resume the next state if it was previously paused
                Debug.Log("Resuming paused state: " + nextState.name);
                yield return nextState.Resume();
                PausedStates.Pop();
            }
            else
            {
                // Enter the new state if it was not paused
                yield return CurrentState.Enter(this);
            }
        }
    }

    // Checks if the specified state is in the paused states stack
    private bool IsStateInPausedStack(State nextState)
    {
        //if(PausedStates.Count > 0)
        //Debug.Log("peek : " + PausedStates.Peek()+ "  nextState : "+ nextState);
        return PausedStates.Count > 0 && PausedStates.Peek() == nextState;
    }
}
