using ProjectCore.UI;

namespace ProjectCore.StateMachine
{
    public interface IState
    {
        void TransitionTo(Transition transition, UICloseReasons closeReason = UICloseReasons.ResumeAny);
    }
}
