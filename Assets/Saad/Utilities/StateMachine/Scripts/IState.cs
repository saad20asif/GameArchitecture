using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectCore.StateMachine
{
    public interface IState
    {
        void TransitionTo(Transition transition);
    }
}
