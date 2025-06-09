using System;
using ProjectCore.UI;
using System.Collections;
using UnityEngine;
using ProjectCore.Events;
using ProjectCore.StateMachine;

public class ApplicationFlowController : MonoBehaviour
{
    [SerializeField] private FiniteStateMachine FiniteStateMachine;

    [Header("Common")] 
    [SerializeField] private GameEvent backBtnPressedEvent;
    
    
    [Header("MainMenu")]
    [SerializeField] private GameEventWithInt GoToMainMenuEvent;
    [SerializeField] private Transition MainMenuTransition;

    [Header("SpinWheel")]
    [SerializeField] private GameEvent GoToSpinWheelEvent;
    [SerializeField] private Transition SpinWheelTransition;

    [Header("GameState")]
    [SerializeField] private GameEvent GoToGameEvent;
    [SerializeField] private Transition GameTransition;

    [Header("LevelComplete")]
    [SerializeField] private GameEvent GoToLevelCompleteEvent;
    [SerializeField] private Transition LevelCompleteTransition;

    [Header("LevelFail")]
    [SerializeField] private GameEvent GoToLevelFailEvent;
    [SerializeField] private Transition LevelFailTransition;

    [Header("RateUs")]
    [SerializeField] private GameEvent GoToRateUsEvent;
    [SerializeField] private Transition RateUsTransition;

    private void OnEnable()
    {
        backBtnPressedEvent.Subscribe(OnBackButtonPressed);
        GoToMainMenuEvent.Subscribe(GoToMainMenu);
        GoToSpinWheelEvent.Subscribe(GoToSpinWheel);
        GoToGameEvent.Subscribe(GoToGame);
        GoToLevelCompleteEvent.Subscribe(GoToLevelComplete);
        GoToLevelFailEvent.Subscribe(GoToLevelFail);
        GoToRateUsEvent.Subscribe(GoToRateUs);
    }

    private void OnDisable()
    {
        backBtnPressedEvent.UnSubscribe(OnBackButtonPressed);
        GoToMainMenuEvent.UnSubscribe(GoToMainMenu);
        GoToSpinWheelEvent.UnSubscribe(GoToSpinWheel);
        GoToGameEvent.UnSubscribe(GoToGame);
        GoToLevelCompleteEvent.UnSubscribe(GoToLevelComplete);
        GoToLevelFailEvent.UnSubscribe(GoToLevelFail);
        GoToRateUsEvent.UnSubscribe(GoToRateUs);
    }

    public void Boot() => GoToMainMenu(0);
    
    private ClosePolicy GetPolicyForReason(UICloseReasons reason)
    {
        switch (reason)
        {
            case UICloseReasons.Home:
                return ClosePolicy.ClearAll;
            case UICloseReasons.Game:
                return ClosePolicy.ClearAll;
            case UICloseReasons.ResumeGame:
                return ClosePolicy.PopOne;
            case UICloseReasons.ShowFullScreenPlacement:
                return ClosePolicy.Default;
            default:
                return ClosePolicy.Default;
        }
    }
    private bool ShouldPauseCurrent(UICloseReasons reason)
    {
        return reason == UICloseReasons.ShowFullScreenPlacement; // Pause for ads, not for others
    }
    private IEnumerator HandleTransition(Transition transition, ClosePolicy policy, bool pauseCurrent)
    {
        switch (policy)
        {
            case ClosePolicy.ClearAll:
                yield return FiniteStateMachine.ClearPausedStates();
                break;
            case ClosePolicy.PopOne:
                yield return FiniteStateMachine.PopPausedState();
                break;
            case ClosePolicy.PopUntil:
                yield return FiniteStateMachine.JumpTo(transition.ToState);
                break;
        }
        FiniteStateMachine.TransitionTo(transition, pauseCurrent);
    }

    private void GoToMainMenu(int reasonId)
    {
        if (Enum.IsDefined(typeof(UICloseReasons), reasonId))
        {
            UICloseReasons reason = (UICloseReasons)reasonId;
            Debug.Log("reason: " + reason);
            StartCoroutine(GoToMainMenuRoutine());
        }
        else
        {
            Debug.LogError("Invalid UICloseReasons value: " + reasonId);
        }
    }

    private IEnumerator GoToMainMenuRoutine()
    {
        yield return FiniteStateMachine.ClearPausedStates();
        FiniteStateMachine.TransitionTo(MainMenuTransition);
    }
    public void OnBackButtonPressed()
    {
        StartCoroutine(FiniteStateMachine.PopPausedState());
    }
    private void GoToSpinWheel()
    {
        FiniteStateMachine.TransitionTo(SpinWheelTransition, pauseCurrent: true);
    }

    private void GoToGame()
    {
        StartCoroutine(GoToGameRoutine());
    }

    private IEnumerator GoToGameRoutine()
    {
        yield return FiniteStateMachine.PopPausedState();
        FiniteStateMachine.TransitionTo(GameTransition);
    }

    private void GoToLevelComplete()
    {
        FiniteStateMachine.TransitionTo(LevelCompleteTransition, pauseCurrent: true);
    }

    private void GoToLevelFail()
    {
        FiniteStateMachine.TransitionTo(LevelFailTransition, pauseCurrent: true);
    }

    private void GoToRateUs()
    {
        FiniteStateMachine.TransitionTo(RateUsTransition, pauseCurrent: true);
    }
    
}