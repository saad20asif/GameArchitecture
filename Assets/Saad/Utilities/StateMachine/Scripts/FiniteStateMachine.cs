using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using THEBADDEST.Coroutines;
using UnityEngine;

namespace Blues.Core.StateMachine
{
    // ScriptableObject-based FSM controller. Manages entering, exiting, pausing, and resuming states.
    [CreateAssetMenu(fileName = "FiniteStateMachine", menuName = "ProjectCore/State Machine/Basic FSM")]
    public class FiniteStateMachine : SerializedScriptableObject, IState
    {
        // State that the FSM boots into when first initialized
        [SerializeField] private State BootState;

        // Currently active state (top of the stack)
        [SerializeField] private State CurrentState;

        // Stack of paused states (e.g., for pop-up overlays, full-screen interruptions)
        [SerializeField] private Stack<State> PausedStates = new();

        // Lookup used to quickly check if a state is paused (for safety)
        private readonly HashSet<State> _pausedStateLookup = new();

        // Tracks sort order for UI layering of state views.
        // Plain int — NOT a ScriptableObject. Resets to 0 on every play session automatically.
        private int _sortingOrder;
        public int CurrentSortingOrder => _sortingOrder;

        // Handle for an active coroutine so we can cancel overlapping transitions
        private Coroutine _transitionCoroutine;

        // Events for tracking state lifecycle externally (used by observers like UI managers)
        public event System.Action<State> OnStateEntered;
        public event System.Action<State> OnStateExited;
        public event System.Action<State> OnStatePaused;
        public event System.Action<State> OnStateResumed;

        // Called to boot the FSM and enter the initial state
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

            PausedStates.Clear(); // Ensure no leftover paused states
        }

        // Begin a transition to a new state (optionally pause current)
        // IState contract
        public void TransitionTo(Transition transition, bool pauseCurrent = false)
        {
            if (transition == null || transition.ToState == null)
            {
                Debug.LogWarning("Invalid transition or target state.");
                return;
            }

            // Cancel existing transition if active
            if (_transitionCoroutine != null)
                CoroutineHandler.StopStaticCoroutine(_transitionCoroutine);

            // Start new transition coroutine
            _transitionCoroutine = CoroutineHandler.StartStaticCoroutine(
                DoTransition(transition, pauseCurrent));
        }

        // Core transition coroutine
        private IEnumerator DoTransition(Transition transition, bool pauseCurrent)
        {
            var nextState = transition.ToState;

            // If transitioning to the same state, skip
            if (CurrentState == nextState)
            {
                Debug.Log($"Already in state {nextState.name}, skipping redundant transition.");
                yield break;
            }

            // Prevent transitioning to a paused state; must resume manually
            if (_pausedStateLookup.Contains(nextState))
            {
                Debug.LogWarning($"State '{nextState.name}' is paused. Use ResumePausedState or JumpTo instead.");
                yield break;
            }

            // Exit or pause the current state
            if (pauseCurrent || nextState.PausePreviousState)
            {
                yield return PauseCurrentState();
            }
            else
            {
                yield return ExitCurrentState();
            }

            yield return transition.Execute();
            CurrentState = nextState;
            OnStateEntered?.Invoke(CurrentState);
            yield return nextState.Enter(this);
        }

        // Reload the current state (exit → enter again)
        public IEnumerator ReloadCurrentState()
        {
            Debug.Log($"Reloading state: {CurrentState.name}");

            OnStateExited?.Invoke(CurrentState);
            yield return CurrentState.Exit();

            OnStateEntered?.Invoke(CurrentState);
            yield return CurrentState.Enter(this);
        }

        // Pauses the current state, exits it, clears all paused states, then enters the new state.
        // Because states are paused before exit, their views handle the rest (snap instantly).
        public void ClearAllAndTransitionTo(Transition transition)
        {
            if (transition == null || transition.ToState == null)
            {
                Debug.LogWarning("Invalid transition or target state.");
                return;
            }

            if (_transitionCoroutine != null)
                CoroutineHandler.StopStaticCoroutine(_transitionCoroutine);

            _transitionCoroutine = CoroutineHandler.StartStaticCoroutine(
                DoClearAllAndTransition(transition));
        }

        private IEnumerator DoClearAllAndTransition(Transition transition)
        {
            // Pause the current state, then exit it
            if (CurrentState != null)
            {
                yield return CurrentState.Pause();
                OnStatePaused?.Invoke(CurrentState);
                yield return CurrentState.Exit();
                OnStateExited?.Invoke(CurrentState);
            }

            // Clear remaining paused states
            yield return ClearPausedStates();

            // Enter the new state
            yield return transition.Execute();
            CurrentState = transition.ToState;
            OnStateEntered?.Invoke(CurrentState);
            yield return CurrentState.Enter(this);
        }

        // Clears the entire paused state stack

        public IEnumerator ClearPausedStates()
        {
            while (PausedStates.Count > 0)
            {
                _sortingOrder--;
                var paused = PausedStates.Pop();
                _pausedStateLookup.Remove(paused);
                yield return paused.Exit();
                OnStateExited?.Invoke(paused);
            }
        }

        // Pops only the most recent paused state and resumes it (ClosePolicy.PopOne)
        public IEnumerator PopPausedState()
        {
            if (PausedStates.Count == 0)
            {
                Debug.LogWarning("No paused states to pop.");
                yield break;
            }

            var topState = PausedStates.Peek();

            yield return ExitCurrentState();      // Exit current
            yield return ResumePausedState(topState); // Resume previous

            CurrentState = topState;
        }

        // Jumps to a specific state in the paused stack (ClosePolicy.PopUntil)
        public IEnumerator JumpTo(State target)
        {
            if (_pausedStateLookup.Contains(target))
            {
                // 1. Exit current (visible) state — this one animates
                yield return ExitCurrentState();

                // 2. Snap-clear intermediate paused states above the target (no animation)
                while (PausedStates.Peek() != target)
                {
                    var popped = PausedStates.Pop();
                    _pausedStateLookup.Remove(popped);
                    _sortingOrder--;
                    yield return popped.Exit();
                    OnStateExited?.Invoke(popped);
                }

                // 3. Resume the target
                yield return ResumePausedState(target);
                CurrentState = target;
            }
            else
            {
                // Target wasn't paused — exit current (animated), snap-clear stack, enter fresh
                yield return ExitCurrentState();
                yield return ClearPausedStates();

                CurrentState = target;
                yield return CurrentState.Enter(this);
                OnStateEntered?.Invoke(CurrentState);
            }
        }

        // Pauses the current state and pushes it to the paused stack
        private IEnumerator PauseCurrentState()
        {
#if UNITY_EDITOR
            if (PausedStates.Contains(CurrentState))
            {
                Debug.LogError($"State '{CurrentState.name}' is already in the paused stack! Cannot push again.");
            }
#endif
            _sortingOrder++;
            yield return CurrentState.Pause();
            OnStatePaused?.Invoke(CurrentState);

            PausedStates.Push(CurrentState);
            _pausedStateLookup.Add(CurrentState);
        }

        // Fully exits the current state
        private IEnumerator ExitCurrentState()
        {
            if (CurrentState != null)
            {
                yield return CurrentState.Exit();
                OnStateExited?.Invoke(CurrentState);
            }
        }

        // Resumes a paused state and pops it from the paused stack
        private IEnumerator ResumePausedState(State target)
        {
            if (PausedStates.Peek() != target)
            {
                Debug.LogWarning($"Trying to resume non-top state '{target.name}'. This is not allowed.");
                yield break;
            }

            _sortingOrder--;
            yield return target.Resume();
            OnStateResumed?.Invoke(target);

            PausedStates.Pop();
            _pausedStateLookup.Remove(target);
        }
    }
}
