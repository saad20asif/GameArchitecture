using System;
using System.Collections;
using UnityEngine;
using ProjectCore.Events;
using ProjectCore.StateMachine;
using ProjectCore.UI;

public class ApplicationFlowController : MonoBehaviour
{
    // Reference to the finite state machine that handles state logic
    [SerializeField] private FiniteStateMachine FiniteStateMachine;

    // Generic UI events and state transition triggers
    [Header("Common")]
    [SerializeField] private GameEvent backBtnPressedEvent;

    // Event and transition for Main Menu
    [Header("MainMenu")]
    [SerializeField] private GameEventWithInt GoToMainMenuEvent;
    [SerializeField] private Transition MainMenuTransition;

    // Spin wheel screen
    [Header("SpinWheel")]
    [SerializeField] private GameEvent GoToSpinWheelEvent;
    [SerializeField] private Transition SpinWheelTransition;

    // Gameplay state
    [Header("GameState")]
    [SerializeField] private GameEvent GoToGameEvent;
    [SerializeField] private Transition GameTransition;

    // Level Complete screen
    [Header("LevelComplete")]
    [SerializeField] private GameEvent GoToLevelCompleteEvent;
    [SerializeField] private Transition LevelCompleteTransition;

    // Level Fail screen
    [Header("LevelFail")]
    [SerializeField] private GameEvent GoToLevelFailEvent;
    [SerializeField] private Transition LevelFailTransition;

    // Rate Us popup
    [Header("RateUs")]
    [SerializeField] private GameEvent GoToRateUsEvent;
    [SerializeField] private Transition RateUsTransition;

    private void OnEnable()
    {
        // Subscribe to all events to handle state transitions
        backBtnPressedEvent.Subscribe(OnBackButtonPressed);
        GoToMainMenuEvent.Subscribe(GoToMainMenu);
        GoToSpinWheelEvent.Subscribe(() => GoTo(SpinWheelTransition, UICloseReasons.FullScreenPlacement));
        GoToGameEvent.Subscribe(() => GoTo(GameTransition, UICloseReasons.Game));
        GoToLevelCompleteEvent.Subscribe(() => GoTo(LevelCompleteTransition, UICloseReasons.FullScreenPlacement));
        GoToLevelFailEvent.Subscribe(() => GoTo(LevelFailTransition, UICloseReasons.FullScreenPlacement));
        GoToRateUsEvent.Subscribe(() => GoTo(RateUsTransition, UICloseReasons.FullScreenPlacement));
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        backBtnPressedEvent.UnSubscribe(OnBackButtonPressed);
        GoToMainMenuEvent.UnSubscribe(GoToMainMenu);
        GoToSpinWheelEvent.UnSubscribe(() => GoTo(SpinWheelTransition, UICloseReasons.FullScreenPlacement));
        GoToGameEvent.UnSubscribe(() => GoTo(GameTransition, UICloseReasons.Game));
        GoToLevelCompleteEvent.UnSubscribe(() => GoTo(LevelCompleteTransition, UICloseReasons.FullScreenPlacement));
        GoToLevelFailEvent.UnSubscribe(() => GoTo(LevelFailTransition, UICloseReasons.FullScreenPlacement));
        GoToRateUsEvent.UnSubscribe(() => GoTo(RateUsTransition, UICloseReasons.FullScreenPlacement));
    }

    // Called when app starts — boots to main menu
    public void Boot()
    {
        StartCoroutine(HandleTransition(MainMenuTransition, GetPolicyForReason(UICloseReasons.Home), ShouldPauseCurrent(UICloseReasons.Home)));
    }

    // Convert int to enum safely and handle main menu navigation
    private void GoToMainMenu(int reasonId)
    {
        if (!Enum.IsDefined(typeof(UICloseReasons), reasonId))
        {
            Debug.LogError($"Invalid UICloseReasons value: {reasonId}");
            return;
        }

        UICloseReasons reason = (UICloseReasons)reasonId;
        GoTo(MainMenuTransition, reason);
    }

    // Generic transition handler using FSM and close policy
    private void GoTo(Transition transition, UICloseReasons reason)
    {
        StartCoroutine(HandleTransition(
            transition,
            GetPolicyForReason(reason),   // Determine how to clean up paused states
            ShouldPauseCurrent(reason)    // Determine if current state should be paused
        ));
    }

    // Performs the transition based on close policy logic
    private IEnumerator HandleTransition(Transition transition, ClosePolicy defaultPolicy, bool pauseCurrent)
    {
        // If transition overrides default policy, use that instead
        var policy = transition.closePolicy != ClosePolicy.Default ? transition.closePolicy : defaultPolicy;
        Debug.Log("policy : " + policy);

        // Execute state cleanup depending on policy
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
                yield break; // Skip FSM.TransitionTo since JumpTo already resumes the state
        }

        // Apply transition (pause or replace current)
        FiniteStateMachine.TransitionTo(transition, pauseCurrent);
    }

    // Handles back button behavior (usually resume previous UI state)
    private void OnBackButtonPressed()
    {
        StartCoroutine(FiniteStateMachine.PopPausedState());
    }

    // Maps logical reason to stack close behavior
    private ClosePolicy GetPolicyForReason(UICloseReasons reason)
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

    // Determines whether to pause current state before pushing new one
    private bool ShouldPauseCurrent(UICloseReasons reason)
    {
        return reason == UICloseReasons.ShowFullScreenPlacement ||
               reason == UICloseReasons.FullScreenPlacement ||
               reason == UICloseReasons.DailyLogin;
    }
}
