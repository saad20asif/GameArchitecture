using Blues.Core.Events;
using Blues.Core.StateMachine;
using Blues.Core.UI;
using UnityEngine;

public class ApplicationFlowController : BaseApplicationFlowController<Transition>
{
    [Header("MainMenu")]
    [SerializeField] private GameEventWithInt GoToMainMenuEvent;
    [SerializeField] private Transition MainMenuTransition;

    [Header("SpinWheel")]
    [SerializeField] private GameEvent GoToSpinWheelEvent;
    [SerializeField] private Transition SpinWheelTransition;

    [Header("Game")]
    [SerializeField] private GameEvent GoToGameEvent;
    [SerializeField] private Transition GameTransition;

    [Header("Level Complete / Fail")]
    [SerializeField] private GameEvent GoToLevelCompleteEvent;
    [SerializeField] private Transition LevelCompleteTransition;
    [SerializeField] private GameEvent GoToLevelFailEvent;
    [SerializeField] private Transition LevelFailTransition;

    [Header("Rate Us")]
    [SerializeField] private GameEvent GoToRateUsEvent;
    [SerializeField] private Transition RateUsTransition;
    public void Boot()
    {
        BootFlow(MainMenuTransition, UICloseReasons.Home);
    }

    protected override void RegisterFlowEvents()
    {
        GoToMainMenuEvent.Subscribe(GoToMainMenu);
        GoToSpinWheelEvent.Subscribe(HandleGoToSpinWheel);
        GoToGameEvent.Subscribe(HandleGoToGame);
        GoToLevelCompleteEvent.Subscribe(HandleGoToLevelComplete);
        GoToLevelFailEvent.Subscribe(HandleGoToLevelFail);
        GoToRateUsEvent.Subscribe(HandleGoToRateUs);
}

    protected override void UnregisterFlowEvents()
    {
        GoToMainMenuEvent.UnSubscribe(GoToMainMenu);
        GoToSpinWheelEvent.UnSubscribe(HandleGoToSpinWheel);
        GoToGameEvent.UnSubscribe(HandleGoToGame);
        GoToLevelCompleteEvent.UnSubscribe(HandleGoToLevelComplete);
        GoToLevelFailEvent.UnSubscribe(HandleGoToLevelFail);
        GoToRateUsEvent.UnSubscribe(HandleGoToRateUs);
}

    private void HandleGoToSpinWheel()    => GoTo(SpinWheelTransition, UICloseReasons.FullScreenPlacement);
    private void HandleGoToGame()         => GoTo(GameTransition, UICloseReasons.Game);
    private void HandleGoToLevelComplete()=> GoTo(LevelCompleteTransition, UICloseReasons.FullScreenPlacement);
    private void HandleGoToLevelFail()    => GoTo(LevelFailTransition, UICloseReasons.FullScreenPlacement);
    private void HandleGoToRateUs()       => GoTo(RateUsTransition, UICloseReasons.FullScreenPlacement);

    private void GoToMainMenu(int reasonId)
    {
        if (!System.Enum.IsDefined(typeof(UICloseReasons), reasonId))
        {
            Debug.LogError($"Invalid UICloseReasons: {reasonId}");
            return;
        }

        GoTo(MainMenuTransition, (UICloseReasons)reasonId);
    }
}
