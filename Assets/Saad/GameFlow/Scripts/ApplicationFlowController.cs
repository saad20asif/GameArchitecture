using UnityEngine;

public class ApplicationFlowController : MonoBehaviour
{
    [SerializeField] private FiniteStateMachine FiniteStateMachine;

    [Header("MainMenu")]
    [SerializeField] private GameEvent GoToMainMenuEvent;
    [SerializeField] private Transition MainMenuTransition;

    [Header("GameState")]
    [SerializeField] private GameEvent GoToGameEvent;
    [SerializeField] private Transition GameTransition;

    [Header("LevelComplete")]
    [SerializeField] private GameEvent GoToLevelCompleteEvent;
    [SerializeField] private Transition LevelCompleteTransition;

    [Header("LevelFail")]
    [SerializeField] private GameEvent GoToLevelFailEvent;
    [SerializeField] private Transition LevelFailTransition;

    private void OnEnable()
    {
        GoToMainMenuEvent.Handler += GoToMainMenu;
        GoToGameEvent.Handler += GoToGame;
        GoToLevelCompleteEvent.Handler += GoToLevelComplete;
        GoToLevelFailEvent.Handler += GoToLevelFail;
    }

    private void OnDisable()
    {
        GoToMainMenuEvent.Handler -= GoToMainMenu;
        GoToGameEvent.Handler -= GoToGame;
        GoToLevelCompleteEvent.Handler -= GoToLevelComplete;
        GoToLevelFailEvent.Handler -= GoToLevelFail;
    }
    public void Boot()
    {
        GoToMainMenu();
    }

    private void GoToMainMenu()
    {
        FiniteStateMachine.TransitionTo(MainMenuTransition);
    }

    private void GoToGame()
    {
        FiniteStateMachine.TransitionTo(GameTransition);
    }

    private void GoToLevelComplete()
    {
        FiniteStateMachine.TransitionTo(LevelCompleteTransition);
    }

    private void GoToLevelFail()
    {
        FiniteStateMachine.TransitionTo(LevelFailTransition);
    }

}
