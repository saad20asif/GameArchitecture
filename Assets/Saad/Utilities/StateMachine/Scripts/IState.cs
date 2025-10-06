using System.Collections;

namespace Blues.Core.StateMachine
{
    public interface IState
    {
        void TransitionTo(Transition transition, bool pauseCurrent = false);
        IEnumerator ClearPausedStates();
        IEnumerator ReloadCurrentState();
    }
}