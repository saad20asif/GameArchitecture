using UnityEngine;
using ProjectCore.StateMachine;
using ProjectCore.Events;

namespace ProjectCore.Application
{
    public class ApplicationFlowController : MonoBehaviour
    {
        [SerializeField] private FiniteStateMachine FiniteStateMachine;

        [Header("MainMenu")]
        [SerializeField] private GameEvent GoToMainMenuEvent;
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

        private void OnEnable()
        {
            GoToMainMenuEvent.Subscribe(GoToMainMenu);
            GoToSpinWheelEvent.Subscribe(GoToSpinWheel);
            GoToGameEvent.Subscribe(GoToGame);
            GoToLevelCompleteEvent.Subscribe(GoToLevelComplete);
            GoToLevelFailEvent.Subscribe(GoToLevelFail);
        }

        private void OnDisable()
        {
            GoToMainMenuEvent.UnSubscribe(GoToMainMenu);
            GoToSpinWheelEvent.UnSubscribe(GoToSpinWheel);
            GoToGameEvent.UnSubscribe(GoToGame);
            GoToLevelCompleteEvent.UnSubscribe(GoToLevelComplete);
            GoToLevelFailEvent.UnSubscribe(GoToLevelFail);
        }
        public void Boot()
        {
            GoToMainMenu();
        }

        private void GoToMainMenu()
        {
            FiniteStateMachine.TransitionTo(MainMenuTransition);
        }
        private void GoToSpinWheel()
        {
            FiniteStateMachine.TransitionTo(SpinWheelTransition);
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
}
