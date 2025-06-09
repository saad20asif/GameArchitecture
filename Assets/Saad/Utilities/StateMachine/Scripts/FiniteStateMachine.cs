using ProjectCore.Variables;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using THEBADDEST.Coroutines;
using UnityEngine;

namespace ProjectCore.StateMachine
{
    [CreateAssetMenu(fileName = "FiniteStateMachine", menuName = "ProjectCore/State Machine/Basic FSM")]
    public class FiniteStateMachine : SerializedScriptableObject, IState
    {
        [SerializeField] private State BootState;
        [SerializeField] private State CurrentState;
        [SerializeField] private Stack<State> PausedStates = new();
        private readonly HashSet<State> _pausedStateLookup = new();

        [SerializeField] private Int currentStateSortingOrder;
        private Coroutine _transitionCoroutine;

        public event System.Action<State> OnStateEntered;
        public event System.Action<State> OnStateExited;
        public event System.Action<State> OnStatePaused;
        public event System.Action<State> OnStateResumed;

        public IEnumerator Init()
        {
            if (BootState == null)
            {
                Debug.LogWarning("Please assign a boot state in FiniteStateMachine to start the game!");
                yield break;
            }

            CurrentState = BootState;
            yield return CurrentState.Enter(this);
            OnStateEntered?.Invoke(CurrentState);
            PausedStates.Clear();
        }

        public void TransitionTo(Transition transition, bool pauseCurrent = false)
        {
            if (transition == null || transition.ToState == null)
            {
                Debug.LogWarning("Invalid transition or target state.");
                return;
            }

            if (_transitionCoroutine != null)
                CoroutineHandler.StopStaticCoroutine(_transitionCoroutine);

            _transitionCoroutine = CoroutineHandler.StartStaticCoroutine(DoTransition(transition, pauseCurrent));
        }

        private IEnumerator DoTransition(Transition transition, bool pauseCurrent)
        {
            var nextState = transition.ToState;

            if (CurrentState == nextState)
            {
                Debug.Log($"Already in state {nextState.name}, skipping redundant transition.");
                yield break;
            }

            if (_pausedStateLookup.Contains(nextState))
            {
                Debug.LogWarning($"State '{nextState.name}' is paused. Use ResumePausedState or JumpTo instead.");
                yield break;
            }

            if (pauseCurrent || nextState.PausePreviousState)
            {
                yield return PauseCurrentState();
            }
            else
            {
                yield return ExitCurrentState();
            }

            yield return transition.Execute();
            yield return nextState.Enter(this);

            CurrentState = nextState;
            OnStateEntered?.Invoke(CurrentState);
        }

        public IEnumerator ReloadCurrentState()
        {
            Debug.Log($"Reloading state: {CurrentState.name}");
            OnStateExited?.Invoke(CurrentState);
            yield return CurrentState.Exit();
            OnStateEntered?.Invoke(CurrentState);
            yield return CurrentState.Enter(this);
        }
        public IEnumerator ClearPausedStates()
        {
            while (PausedStates.Count > 0)
            {
                currentStateSortingOrder.Decrement(1);
                var paused = PausedStates.Pop();
                _pausedStateLookup.Remove(paused);
                yield return paused.Exit();
                OnStateExited?.Invoke(paused);
            }
        }

        public IEnumerator PopPausedState()
        {
            if (PausedStates.Count == 0)
            {
                Debug.LogWarning("No paused states to pop.");
                yield break;
            }

            var topState = PausedStates.Peek();
            yield return ExitCurrentState();
            yield return ResumePausedState(topState);
            CurrentState = topState;
        }

        public IEnumerator JumpTo(State target)
        {
            if (_pausedStateLookup.Contains(target))
            {
                while (PausedStates.Peek() != target)
                {
                    var popped = PausedStates.Pop();
                    _pausedStateLookup.Remove(popped);
                    currentStateSortingOrder.Decrement(1);
                    yield return popped.Exit();
                    OnStateExited?.Invoke(popped);
                }

                yield return ExitCurrentState();
                yield return ResumePausedState(target);
                CurrentState = target;
            }
            else
            {
                yield return ExitCurrentState();
                yield return ClearPausedStates();
                CurrentState = target;
                yield return CurrentState.Enter(this);
                OnStateEntered?.Invoke(CurrentState);
            }
        }

        private IEnumerator PauseCurrentState()
        {
#if UNITY_EDITOR
            if (PausedStates.Contains(CurrentState))
            {
                Debug.LogError($"State '{CurrentState.name}' is already in the paused stack! Cannot push again.");
            }
#endif
            currentStateSortingOrder.Increment(1);
            yield return CurrentState.Pause();
            OnStatePaused?.Invoke(CurrentState);
            PausedStates.Push(CurrentState);
            _pausedStateLookup.Add(CurrentState);
        }

        private IEnumerator ExitCurrentState()
        {
            if (CurrentState != null)
            {
                yield return CurrentState.Exit();
                OnStateExited?.Invoke(CurrentState);
            }
        }

        private IEnumerator ResumePausedState(State target)
        {
            if (PausedStates.Peek() != target)
            {
                Debug.LogWarning($"Trying to resume non-top state '{target.name}'. This is not allowed.");
                yield break;
            }

            currentStateSortingOrder.Decrement(1);
            yield return target.Resume();
            OnStateResumed?.Invoke(target);
            PausedStates.Pop();
            _pausedStateLookup.Remove(target);
        }
    }
}