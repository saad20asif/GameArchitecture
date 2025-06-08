using System.Collections;

namespace ProjectCore.StateMachine
{
    public interface IState
    {
        void TransitionTo(Transition transition, bool pauseCurrent = false);
        IEnumerator ClearPausedStates();
        IEnumerator ReloadCurrentState();
    }
}