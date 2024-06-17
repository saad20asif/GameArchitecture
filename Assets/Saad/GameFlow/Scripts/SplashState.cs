using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SplashState", menuName = "ProjectCore/States/SplashState")]
public class SplashState : State
{
    AsyncOperation _asyncLoad;
    [SerializeField] private Float SceneLoadingProgress;
    private ApplicationFlowController _applicationFlowController;
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

    }
    public override IEnumerator Exit()
    {
        Debug.Log("Splash State Exit has been called!");
        yield return base.Exit();
    }
    private IEnumerator GameSceneLoading()
    {
        _asyncLoad = SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
        _asyncLoad.allowSceneActivation = false;
        yield break;
    }

}
