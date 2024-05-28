using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    void TransitionTo(State state, Transition transition);
}
