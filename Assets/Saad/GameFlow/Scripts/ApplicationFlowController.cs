using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationFlowController : MonoBehaviour
{
    [SerializeField] private FiniteStateMachine FiniteStateMachine;

    [Header("MainMenu")]
    [SerializeField] private GameEvent GoToMainMenuEvent;
    [SerializeField] private Transition MainMenuTransition;
    private Coroutine _transitionCo;

    public void Boot()
    {
        print("Application Base Called!");
        GoToMainMenuEvent.Invoke();
    }

    private void OnEnable()
    {
        GoToMainMenuEvent.Handler += GoToMainMenu;
    }
    private void OnDisable()
    {
        GoToMainMenuEvent.Handler -= GoToMainMenu;
    }
    private void GoToMainMenu()
    {
        Debug.LogError("GoToMainMenuCalled!");
        if (_transitionCo != null)
            StopCoroutine(_transitionCo);
        _transitionCo = StartCoroutine(FiniteStateMachine.DoTransition(MainMenuTransition));
    }
}
