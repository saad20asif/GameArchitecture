using System;
using System.Collections;
using UnityEngine;
using Blues.Core.StateMachine;
using Blues.Core.Events;
using Blues.Core.UI;

public abstract class BaseApplicationFlowController<TTransition> : MonoBehaviour where TTransition : Transition
{
    [SerializeField] protected FiniteStateMachine finiteStateMachine;

    [Header("Events")]
    [SerializeField] private GameEvent backBtnPressedEvent;

    protected virtual void OnEnable()
    {
        backBtnPressedEvent.Subscribe(OnBackButtonPressed);
        RegisterFlowEvents();
    }

    protected virtual void OnDisable()
    {
        backBtnPressedEvent.UnSubscribe(OnBackButtonPressed);
        UnregisterFlowEvents();
    }

    public virtual void BootFlow(TTransition bootTransition, UICloseReasons reason)
    {
        StartCoroutine(HandleTransition(bootTransition, GetPolicyForReason(reason), ShouldPauseCurrent(reason)));
    }

    protected virtual void GoTo(TTransition transition, UICloseReasons reason)
    {
        StartCoroutine(HandleTransition(
            transition,
            GetPolicyForReason(reason),
            ShouldPauseCurrent(reason)
        ));
    }

    protected virtual IEnumerator HandleTransition(TTransition transition, ClosePolicy defaultPolicy, bool pauseCurrent)
    {
        var policy = transition.closePolicy != ClosePolicy.Default ? transition.closePolicy : defaultPolicy;

        switch (policy)
        {
            case ClosePolicy.ClearAll:
                finiteStateMachine.ClearAllAndTransitionTo(transition);
                yield break;

            case ClosePolicy.PopOne:
                yield return finiteStateMachine.PopPausedState();
                yield break;

            case ClosePolicy.PopUntil:
                yield return finiteStateMachine.JumpTo((State)transition.ToState);
                yield break;
        }

        finiteStateMachine.TransitionTo(transition, pauseCurrent);
    }

    protected virtual void OnBackButtonPressed()
    {
        StartCoroutine(finiteStateMachine.PopPausedState());
    }

    protected virtual ClosePolicy GetPolicyForReason(UICloseReasons reason)
    {
        return reason switch
        {
            UICloseReasons.Home => ClosePolicy.ClearAll,
            UICloseReasons.Game => ClosePolicy.ClearAll,
            UICloseReasons.ResumeGame => ClosePolicy.PopOne,
            UICloseReasons.Revive => ClosePolicy.PopOne,
            UICloseReasons.SkipLevel => ClosePolicy.PopOne,
            UICloseReasons.ResumeAny => ClosePolicy.PopUntil,
            _ => ClosePolicy.Default
        };
    }

    protected virtual bool ShouldPauseCurrent(UICloseReasons reason)
    {
        return reason is UICloseReasons.ShowFullScreenPlacement
                     or UICloseReasons.FullScreenPlacement
                     or UICloseReasons.DailyLogin;
    }

    // Override to register specific flow events (MainMenu, Game, etc.)
    protected abstract void RegisterFlowEvents();
    protected abstract void UnregisterFlowEvents();
}
