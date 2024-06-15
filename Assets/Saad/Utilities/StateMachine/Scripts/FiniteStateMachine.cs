using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FiniteStateMachine", menuName = "ProjectCore/State Machine/Basic FSM")]
public class FiniteStateMachine : ScriptableObject, IState
{
    [SerializeField] private State BootState;

    void IState.TransitionTo(State state, Transition transition)
    {
        //Transition(transition);
    }
}
