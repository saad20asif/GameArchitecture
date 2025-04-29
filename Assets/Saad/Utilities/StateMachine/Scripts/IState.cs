using ProjectCore.UI;
using System.Collections;

namespace ProjectCore.StateMachine
{
    public interface IState
    {
        void TransitionTo(Transition transition, UICloseReasons closeReason = UICloseReasons.ResumeAny);
        IEnumerator ClearPausedStates();
    }
}
