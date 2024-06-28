using System.Collections;
using UnityEngine;

public class State : ScriptableObject
{
    protected IState Listener;

    // Enter method which accepts an IState listener, which is nothing but FSM refernce, but in a controlled manner
    // Which means you can only access IState Transition method not all FSM prperties!  (Yeah Thats great!!!)
    // We pass the FiniteStateMachine instance (which implements IState) to allow the state to call TransitionTo
    public virtual IEnumerator Enter(IState listener)
    {
        Listener = listener; // Store the listener (FiniteStateMachine) reference
        yield return null;
    }

    public virtual IEnumerator Exit()
    {
        yield return null;
    }
}
