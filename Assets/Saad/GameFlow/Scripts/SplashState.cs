using UnityEngine;
using System.Collections;
using ProjectCore.PoolSystem;
using ProjectCore.StateMachine;
using UnityEngine.SceneManagement;
using ProjectCore.Variables;
namespace ProjectCore.Application
{

    [CreateAssetMenu(fileName = "SplashState", menuName = "ProjectCore/State Machine/States/SplashState")]
    public class SplashState : State
    {
        AsyncOperation _asyncLoad;
        [SerializeField] private Float SceneLoadingProgress;
        [SerializeField] private PoolManagerSO uIStatesPooler;
        [SerializeField] private PoolManagerSO gameStatePooler;
        private ApplicationFlowController _applicationFlowController;
        private string _sceneName = "GameScene";
        public override IEnumerator Enter(IState _listener)
        {
            Debug.Log("Splash State init has been called!");
            yield return base.Enter(_listener);

            _applicationFlowController = Instantiate(Resources.Load<ApplicationFlowController>("ApplicationFlowController"));

            yield return GameSceneLoading();
            // For now im waiting for slider value to reach 1 but later we can also wait for all sdk's to load first(we can map sdk's loading progress from 0-1 and use Int So for that)
            yield return new WaitUntil(() => SceneLoadingProgress.GetValue() >= 1);
            _asyncLoad.allowSceneActivation = true;
            // Wait for the scene to activate
            yield return new WaitUntil(() => _asyncLoad.isDone);
            SetGameSceneAsActiveScene(); // Set the newly loaded game scene as the active scene
            
            SetupStateRoots();
            InitializerPoolers();
            
            _applicationFlowController.Boot();
        }

        private void InitializerPoolers()
        {
            uIStatesPooler.Initialize(StateRootManager.UIPooled);
            gameStatePooler.Initialize(StateRootManager.GameplayPooled);
        }
        private void SetGameSceneAsActiveScene()
        {
            Scene gameScene = SceneManager.GetSceneByName(_sceneName);
            if (gameScene.IsValid())
            {
                SceneManager.SetActiveScene(gameScene);
                Debug.Log("Game scene activated successfully.");
            }
            else
            {
                Debug.LogWarning("Game scene is not valid.");
            }
        }
        public override IEnumerator Exit()
        {
            Debug.Log("Splash State Exit has been called!");
            yield return base.Exit();
        }
        private IEnumerator GameSceneLoading()
        {
            _asyncLoad = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Additive);
            _asyncLoad.allowSceneActivation = false;
            yield break;
        }
        private void SetupStateRoots()
        {
            if (StateRootManager.IsInitialized)
                return;

            // Find the GameScene root
            Scene gameScene = SceneManager.GetSceneByName(_sceneName);
            if (!gameScene.IsValid())
            {
                Debug.LogError("GameScene is invalid. Cannot setup state roots.");
                return;
            }

            // Create root objects under the loaded GameScene
            GameObject ui        =     new GameObject("-------------------UI-------------------");
            GameObject uiPooled    =     new GameObject("POOLED");
            GameObject uiNonPooled =     new GameObject("NON-POOLED");

            uiPooled.transform.parent = uiNonPooled.transform.parent = ui.transform;
  
            GameObject gamePlay  =     new GameObject("----------------GAMEPLAY----------------");
            GameObject gamePlayPooled    =     new GameObject("POOLED");
            GameObject gamePlayNonPooled =     new GameObject("NON-POOLED");

            gamePlayPooled.transform.parent = gamePlayNonPooled.transform.parent = gamePlay.transform;
            //SceneManager.MoveGameObjectToScene(ui, gameScene);
            //SceneManager.MoveGameObjectToScene(gamePlay, gameScene);

            StateRootManager.Initialize(
                ui.transform,
                uiPooled.transform,
                uiNonPooled.transform,
                gamePlay.transform,
                gamePlayPooled.transform,
                gamePlayNonPooled.transform);
        }

    }

}
