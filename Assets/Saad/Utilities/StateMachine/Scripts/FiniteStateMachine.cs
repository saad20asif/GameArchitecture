using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StateMachine",menuName = "ProjectCore/State Machine/Basic FSM")]
public class StateMachine : ScriptableObject, IState
{
    [SerializeField] private State BootState;

    //[ShowInInspector][NonSerialized] protected State CurrentState;
    //[ShowInInspector][NonSerialized] protected State PreviousState;
    [SerializeField] private Stack<State> PausedStates;
    void IState.TransitionTo(State state, Transition transition)
    {
        //Transition(transition);
    }
}
