using UnityEngine;

public class ApplicationFlowController : MonoBehaviour
{
    [SerializeField] private FiniteStateMachine FiniteStateMachine;

    [Header("MainMenu")]
    [SerializeField] private GameEvent GoToMainMenuEvent;
    [SerializeField] private Transition MainMenuTransition;

    [Header("LevelComplete")]
    [SerializeField] private GameEvent GoToLevelCompleteEvent;
    [SerializeField] private Transition LevelcompleteTransition;

    [Header("LevelFail")]
    [SerializeField] private GameEvent GoToLevelFailEvent;
    [SerializeField] private Transition LevelFailTransition;

    private Coroutine _transitionCo;

    public void Boot()
    {
        print("Application Base Called!");
        GoToMainMenu();
    }

    private void OnEnable()
    {
        GoToMainMenuEvent.Handler += GoToMainMenu;
        GoToLevelCompleteEvent.Handler += GoToLevelComplete;
        GoToLevelFailEvent.Handler += GoToLevelFail;
    }
    private void OnDisable()
    {
        GoToMainMenuEvent.Handler -= GoToMainMenu;
        GoToLevelCompleteEvent.Handler -= GoToLevelComplete;
        GoToLevelFailEvent.Handler -= GoToLevelFail;
    }
    private void GoToMainMenu()
    {
        Debug.LogError("GoToMainMenuCalled!");
        if (_transitionCo != null)
            StopCoroutine(_transitionCo);
        _transitionCo = StartCoroutine(FiniteStateMachine.DoTransition(MainMenuTransition));
        
    }
    private void GoToLevelComplete()
    {
        if (_transitionCo != null)
            StopCoroutine(_transitionCo);
        Debug.LogError("GoToLevelComplete!");
        _transitionCo = StartCoroutine(FiniteStateMachine.DoTransition(LevelcompleteTransition));
    }
    private void GoToLevelFail()
    {
        if (_transitionCo != null)
            StopCoroutine(_transitionCo);
        Debug.LogError("GoToLevelComplete!");
        _transitionCo = StartCoroutine(FiniteStateMachine.DoTransition(LevelFailTransition));
    }
}
