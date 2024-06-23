using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SplashState", menuName = "ProjectCore/State Machine/States/SplashState")]
public class SplashState : State
{
    AsyncOperation _asyncLoad;
    [SerializeField] private Float SceneLoadingProgress;
    private ApplicationFlowController _applicationFlowController;
    private string _sceneName="GameScene";
    public override IEnumerator Enter(IState _listener)
    {
        Debug.Log("Splash State init has been called!");
        yield return base.Enter(_listener);
    }
    public override IEnumerator Execute()
    {
        Debug.Log("Splash State Execute has been called!");
        yield return base.Execute();
        _applicationFlowController = Instantiate(Resources.Load<ApplicationFlowController>("ApplicationFlowController"));

        yield return GameSceneLoading();
        // For now im waiting for slider value to reach 1 but later we can also wait for all sdk's to load first(we can map sdk's loading progress from 0-1 and use Int So for that)
        yield return new WaitUntil(() => SceneLoadingProgress.GetValue() >= 1);
        _asyncLoad.allowSceneActivation = true;
        // Wait for the scene to activate
        yield return new WaitUntil(() => _asyncLoad.isDone);
        SetGameSceneAsActiveScene(); // Set the newly loaded game scene as the active scene

        _applicationFlowController.Boot();

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

}
