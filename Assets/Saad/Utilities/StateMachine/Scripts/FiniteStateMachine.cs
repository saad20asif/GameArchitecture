using ProjectCore.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectCore.StateMachine
{
    [CreateAssetMenu(fileName = "FiniteStateMachine", menuName = "ProjectCore/State Machine/Basic FSM")]
    public class FiniteStateMachine : SerializedScriptableObject, IState
    {
        [SerializeField] private State BootState;
        [SerializeField] private State CurrentState;
        [SerializeField] private Stack<State> PausedStates = new Stack<State>();
        public static int CurrentStateSortingOrder = 0;

        private enum ClosePolicy { ClearAll, PopUntil, PopOne }
        
        private static Transform _viewRoot;

        public static Transform ViewRoot
        {
            get
            {
                if (_viewRoot == null)
                {
                    var viewRootGO = new GameObject("[FSM_ViewRoot]");
                    _viewRoot = viewRootGO.transform;
                }
                return _viewRoot;
            }
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
            CoroutineRunner.instance.StartCoroutine(DoTransition(transition, closeReason));
        }

        private IEnumerator DoTransition(Transition transition, UICloseReasons closeReason)
        {
            var nextState = transition.ToState;
            var policy = _closePolicies.ContainsKey(closeReason)
                ? _closePolicies[closeReason]
                : ClosePolicy.PopOne;

            Debug.Log($"Next: {nextState.name}, Policy: {policy}");

            // Already paused?
            if (PausedStates.Contains(nextState))
            {
                switch (policy)
                {
                    case ClosePolicy.PopUntil:
                        yield return JumpTo(nextState);
                        yield break;

                    case ClosePolicy.ClearAll:
                        // exit current then clear all paused
                        yield return CurrentState.Exit();
                        yield return ClearPausedStates();
                        break;

                    case ClosePolicy.PopOne:
                        if (PausedStates.Peek() == nextState)
                        {
                            // exit current, then resume target
                            yield return CurrentState.Exit();
                            yield return ResumePausedState(nextState);
                            CurrentState = nextState;
                            yield break;
                        }
                        else
                        {
                            // exit current, then clear all paused
                            yield return CurrentState.Exit();
                            yield return ClearPausedStates();
                        }
                        break;
                }
            }

            // Fresh transition or after clearing
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
            CurrentStateSortingOrder++;
            yield return CurrentState.Pause();
            PausedStates.Push(CurrentState);
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
                yield return paused.Exit();
            }
        }
        private IEnumerator ResumePausedState(State target)
        {
            CurrentStateSortingOrder--;
            yield return target.Resume();
            PausedStates.Pop();
        }
        // It will keep popping states until it finds the state which needs to be resumed
        public IEnumerator JumpTo(State target)
        {
            if (PausedStates.Contains(target))
            {
                while (PausedStates.Peek() != target)
                {
                    var popped = PausedStates.Pop();
                    CurrentStateSortingOrder--;
                    yield return popped.Exit();
                }
                yield return CurrentState.Exit();
                yield return target.Resume();
                PausedStates.Pop();
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