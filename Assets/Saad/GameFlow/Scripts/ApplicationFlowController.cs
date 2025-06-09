using ProjectCore.Events;
using ProjectCore.StateMachine;
using ProjectCore.UI;
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
        GoToSpinWheelEvent.Subscribe(() => GoTo(SpinWheelTransition, UICloseReasons.FullScreenPlacement));
        GoToGameEvent.Subscribe(() => GoTo(GameTransition, UICloseReasons.Game));
        GoToLevelCompleteEvent.Subscribe(() => GoTo(LevelCompleteTransition, UICloseReasons.FullScreenPlacement));
        GoToLevelFailEvent.Subscribe(() => GoTo(LevelFailTransition, UICloseReasons.FullScreenPlacement));
        GoToRateUsEvent.Subscribe(() => GoTo(RateUsTransition, UICloseReasons.FullScreenPlacement));
    }

    protected override void UnregisterFlowEvents()
    {
        GoToMainMenuEvent.UnSubscribe(GoToMainMenu);
        GoToSpinWheelEvent.UnSubscribe(() => GoTo(SpinWheelTransition, UICloseReasons.FullScreenPlacement));
        GoToGameEvent.UnSubscribe(() => GoTo(GameTransition, UICloseReasons.Game));
        GoToLevelCompleteEvent.UnSubscribe(() => GoTo(LevelCompleteTransition, UICloseReasons.FullScreenPlacement));
        GoToLevelFailEvent.UnSubscribe(() => GoTo(LevelFailTransition, UICloseReasons.FullScreenPlacement));
        GoToRateUsEvent.UnSubscribe(() => GoTo(RateUsTransition, UICloseReasons.FullScreenPlacement));
    }

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
