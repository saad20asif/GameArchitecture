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
        
        [Header("RateUs")]
        [SerializeField] private GameEvent GoToRateUsEvent;
        [SerializeField] private Transition RateUsTransition;

        private void OnEnable()
        {
            GoToMainMenuEvent.Subscribe(GoToMainMenu);
            GoToSpinWheelEvent.Subscribe(GoToSpinWheel);
            GoToGameEvent.Subscribe(GoToGame);
            GoToLevelCompleteEvent.Subscribe(GoToLevelComplete);
            GoToLevelFailEvent.Subscribe(GoToLevelFail);
            GoToRateUsEvent.Subscribe(GoToRateUs);
        }

        private void OnDisable()
        {
            GoToMainMenuEvent.UnSubscribe(GoToMainMenu);
            GoToSpinWheelEvent.UnSubscribe(GoToSpinWheel);
            GoToGameEvent.UnSubscribe(GoToGame);
            GoToLevelCompleteEvent.UnSubscribe(GoToLevelComplete);
            GoToLevelFailEvent.UnSubscribe(GoToLevelFail);
            GoToRateUsEvent.UnSubscribe(GoToRateUs);
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
        private void GoToRateUs()
        {
            FiniteStateMachine.TransitionTo(RateUsTransition);
        }

    }
}
