using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StateMachine",menuName = "ProjectCore/State Machine/Basic FSM")]
public class FiniteStateMachine : ScriptableObject, IState
{
    void IState.TransitionTo(State state, Transition transition)
    {
        //Transition(transition);
    }
}
