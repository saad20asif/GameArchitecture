using ProjectCore.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using THEBADDEST.Coroutines;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectCore.StateMachine
{
    public enum ClosePolicy
    {
        Default,   // ← Add this for fallback
        ClearAll,
        PopUntil,
        PopOne
    }
    [CreateAssetMenu(fileName = "FiniteStateMachine", menuName = "ProjectCore/State Machine/Basic FSM")]
    public class FiniteStateMachine : SerializedScriptableObject, IState
    {
        [SerializeField] private State BootState;
        [SerializeField] private State CurrentState;
        [SerializeField] private Stack<State> PausedStates = new Stack<State>();
        private readonly HashSet<State> _pausedStateLookup = new HashSet<State>();
        public static int CurrentStateSortingOrder = 0;
        
        private ClosePolicy GetPolicy(UICloseReasons reason, Transition transition)
        {
            if (transition != null && transition.closePolicy != ClosePolicy.Default)
                return transition.closePolicy;

            return _closePolicies.TryGetValue(reason, out var policy) ? policy : ClosePolicy.PopOne;
        }


        private readonly Dictionary<UICloseReasons, ClosePolicy> _closePolicies = new()
        {
            { UICloseReasons.Home, ClosePolicy.ClearAll },
            { UICloseReasons.DailyLogin, ClosePolicy.ClearAll },
            { UICloseReasons.ShowFullScreenPlacement, ClosePolicy.ClearAll },

            { UICloseReasons.ResumeAny, ClosePolicy.PopUntil },
            { UICloseReasons.ResumeGame, ClosePolicy.PopOne },
            { UICloseReasons.Revive, ClosePolicy.PopOne },
            { UICloseReasons.FullScreenPlacement, ClosePolicy.PopOne },

            { UICloseReasons.Game, ClosePolicy.PopUntil },
            { UICloseReasons.SkipLevel, ClosePolicy.PopUntil },
        };

        public IEnumerator Init()
        {
            if (BootState == null)
            {
                Debug.LogWarning("Please assign a boot state in FiniteStateMachine to start the game!");
                yield break;
            }

            CurrentState = BootState;
            yield return CurrentState.Enter(this);
            PausedStates.Clear();
        }

        public void TransitionTo(Transition transition, UICloseReasons closeReason = UICloseReasons.ResumeAny)
        {
            if (transition == null || transition.ToState == null)
            {
                Debug.LogWarning("Invalid transition or target state.");
                return;
            }
            CoroutineHandler.StartStaticCoroutine(DoTransition(transition, closeReason));
        }
        

        private IEnumerator DoTransition(Transition transition, UICloseReasons closeReason)
        {
            var nextState = transition.ToState;
            var policy = GetPolicy(closeReason, transition);


            Debug.Log($"Next: {nextState.name}, Policy: {policy}");

            if (_pausedStateLookup.Contains(nextState))
            {
                yield return HandlePausedTransition(policy, nextState);
                yield break;
            }

            yield return HandleFreshTransition(transition, nextState);
        }
        private IEnumerator HandlePausedTransition(ClosePolicy policy, State nextState)
        {
            switch (policy)
            {
                case ClosePolicy.PopUntil:
                    yield return JumpTo(nextState);
                    break;

                case ClosePolicy.ClearAll:
                    yield return CurrentState.Exit();
                    yield return ClearPausedStates();
                    break;

                case ClosePolicy.PopOne:
                    if (PausedStates.Peek() == nextState)
                    {
                        yield return CurrentState.Exit();
                        yield return ResumePausedState(nextState);
                        CurrentState = nextState;
                    }
                    else
                    {
                        yield return CurrentState.Exit();
                        yield return ClearPausedStates();
                    }
                    break;
            }
        }
        private IEnumerator HandleFreshTransition(Transition transition, State nextState)
        {
            if (nextState.PausePreviousState)
            {
                yield return PauseCurrentState();
            }
            else
            {
                yield return HandleNonPausedState(nextState);
            }

            CurrentState = nextState;
            yield return transition.Execute();
            yield return CurrentState.Enter(this);
        }


        private IEnumerator PauseCurrentState()
        {
            if (_pausedStateLookup.Contains(CurrentState))
            {
                Debug.LogWarning($"Trying to pause state '{CurrentState.name}' which is already paused.");
                yield break;
            }

            CurrentStateSortingOrder++;
            yield return CurrentState.Pause();
            PausedStates.Push(CurrentState);
            _pausedStateLookup.Add(CurrentState);
        }


        private IEnumerator HandleNonPausedState(State nextState)
        {
            if (!IsStateInPausedStack(nextState))
            {
                yield return ClearPausedStates();
            }
            yield return CurrentState.Exit();
        }

        public IEnumerator ClearPausedStates()
        {
            while (PausedStates.Count > 0)
            {
                CurrentStateSortingOrder--;
                var paused = PausedStates.Pop();
                _pausedStateLookup.Remove(paused);
                yield return paused.Exit();
            }
        }

        private IEnumerator ResumePausedState(State target)
        {
            if (PausedStates.Peek() != target)
            {
                Debug.LogWarning($"Trying to resume non-top state '{target.name}'. This is not allowed.");
                yield break;
            }

            CurrentStateSortingOrder--;
            yield return target.Resume();
            PausedStates.Pop();
            _pausedStateLookup.Remove(target);
        }

        // It will keep popping states until it finds the state which needs to be resumed
        public IEnumerator JumpTo(State target)
        {
            if (_pausedStateLookup.Contains(target))
            {
                while (PausedStates.Peek() != target)
                {
                    var popped = PausedStates.Pop();
                    _pausedStateLookup.Remove(popped);
                    CurrentStateSortingOrder--;
                    yield return popped.Exit();
                }

                yield return CurrentState.Exit();
                yield return target.Resume();
                PausedStates.Pop();
                _pausedStateLookup.Remove(target);
                CurrentState = target;
            }
            else
            {
                yield return CurrentState.Exit();
                yield return ClearPausedStates();
                CurrentState = target;
                yield return CurrentState.Enter(this);
            }
        }


        private bool IsStateInPausedStack(State state)
        {
            return PausedStates.Count > 0 && PausedStates.Contains(state);
        }

        
    }
}