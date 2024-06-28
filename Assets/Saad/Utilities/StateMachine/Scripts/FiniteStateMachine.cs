using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "FiniteStateMachine", menuName = "ProjectCore/State Machine/Basic FSM")]
public class FiniteStateMachine : ScriptableObject, IState
{
    [SerializeField] private State BootState;
    [SerializeField] private State CurrentState;

    // Initializes the FiniteStateMachine with the BootState
    public IEnumerator Init()
    {
        Debug.Log("FiniteStateMachine Init called");
        if (BootState != null)
        {
            CurrentState = BootState;
            // Pass 'this' (the FiniteStateMachine instance) to the BootState
            yield return CurrentState.Enter(this);
        }
        else
        {
            Debug.LogWarning("Please assign a boot state in FiniteStateMachine to start the game!");
            yield return null;
        }
    }

    // Implements the TransitionTo method from the IState interface
    public void TransitionTo(Transition transition)
    {
        if (transition != null && transition.ToState != null)
            CoroutineRunner.instance.StartCoroutine(DoTransition(transition));
    }

    // Performs the state transition
    private IEnumerator DoTransition(Transition transition)
    {
        Debug.Log("DoTransition " + transition.ToState.name);
        if (CurrentState != null)
        {
            yield return CurrentState.Exit();
            CurrentState = transition.ToState;
            // Pass 'this' (the FiniteStateMachine instance) to the new state,  so that any state can call Transition directly too!
            yield return CurrentState.Enter(this);
        }
        yield return null;
    }
}
