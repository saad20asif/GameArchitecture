using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SplashState", menuName = "ProjectCore/States/SplashState")]
public class SplashState : State
{
    [SerializeField]private Float SceneLoadingProgress;
    private ApplicationFlowController _applicationFlowController;
    public override IEnumerator Init(IState _listener)
    {
        Debug.Log("Splash State init has been called!");
        yield return base.Init(_listener);
    }
    public override IEnumerator Execute()
    {
        Debug.Log("Splash State Execute has been called!");
        yield return base.Execute();
        _applicationFlowController = Instantiate(Resources.Load<ApplicationFlowController>("ApplicationFlowController"));

    }
    public override IEnumerator Exit()
    {
        Debug.Log("Splash State Exit has been called!");
        yield return base.Exit();
    }

    
}
