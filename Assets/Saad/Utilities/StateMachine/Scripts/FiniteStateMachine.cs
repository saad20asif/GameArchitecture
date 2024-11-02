using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace ProjectCore.StateMachine
{

    [CreateAssetMenu(fileName = "FiniteStateMachine", menuName = "ProjectCore/State Machine/Basic FSM")]
    public class FiniteStateMachine : SerializedScriptableObject, IState
    {
        [SerializeField] private State BootState; // The initial state of the FSM
        [SerializeField] private State CurrentState; // The current active state
        [SerializeField] private Stack<State> PausedStates = new Stack<State>(); // Stack to manage paused states
        public static int CurrentStateSortingOrder = 0;

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

        private IEnumerator DoTransition(Transition transition)
        {
            Debug.Log("DoTransition " + transition.ToState.name);

            if (CurrentState == null)
                yield break;

            State nextState = transition.ToState;

            if (nextState.PausePreviousState && !IsStateInPausedStack(nextState))
            {
                yield return PauseCurrentState();
            }
            else
            {
                yield return HandleNonPausedState(nextState);
            }

            CurrentState = nextState;

            if (IsStateInPausedStack(nextState))
            {
                yield return ResumePausedState(nextState);
            }
            else
            {
                yield return EnterNewState(transition);
            }
        }

        private IEnumerator PauseCurrentState()
        {
            Debug.Log("Pausing current state.");
            CurrentStateSortingOrder++;
            yield return CurrentState.Pause();
            PausedStates.Push(CurrentState);
        }

        private IEnumerator HandleNonPausedState(State nextState)
        {
            if (!IsStateInPausedStack(nextState))
            {
                Debug.Log("Clearing all paused states.");
                yield return ClearPausedStates();
            }

            yield return CurrentState.Exit();
        }

        private IEnumerator ClearPausedStates()
        {
            while (PausedStates.Count > 0)
            {
                CurrentStateSortingOrder--;
                State pausedState = PausedStates.Pop();
                yield return pausedState.Exit();
            }
        }

        private IEnumerator ResumePausedState(State nextState)
        {
            Debug.Log("Resuming paused state: " + nextState.name);
            CurrentStateSortingOrder--;
            yield return nextState.Resume();
            PausedStates.Pop();
        }

        private IEnumerator EnterNewState(Transition transition)
        {
            yield return transition.Execute();
            yield return CurrentState.Enter(this);
        }


        // Checks if the specified state is in the paused states stack
        private bool IsStateInPausedStack(State nextState)
        {
            //if(PausedStates.Count > 0)
            //Debug.Log("peek : " + PausedStates.Peek()+ "  nextState : "+ nextState);
            return PausedStates.Count > 0 && PausedStates.Peek() == nextState;
        }
    }
}
