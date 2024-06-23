using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "FiniteStateMachine", menuName = "ProjectCore/State Machine/Basic FSM")]
public class FiniteStateMachine : ScriptableObject, IState
{
    [SerializeField] private State BootState;
    [SerializeField] private State CurrentState;

    
    public IEnumerator Init()
    {
        Debug.Log("FiniteStateMachine Init called");
        if (BootState != null)
        {
            CurrentState = BootState;
            yield return CurrentState.Enter(this);
            yield return CurrentState.Execute();
        }
        else
        {
            Debug.LogWarning("Plz pass the boot state in FiniteStateMachine to start the game! ");
            yield return null;
        }
    }

    public IEnumerator DoTransition(Transition transition)
    {
        Debug.Log("DoTransition " + transition.ToState.name);
        if(CurrentState != null)
        {
            yield return CurrentState.Exit();
            CurrentState = transition.ToState;
            yield return CurrentState.Enter(this);
            yield return CurrentState.Execute();
        }
        yield return null;
    }

    // Still Cant understand This Interface properly
    void IState.TransitionTo(State state, Transition transition)
    {
        //Transition(transition);
    }
}
