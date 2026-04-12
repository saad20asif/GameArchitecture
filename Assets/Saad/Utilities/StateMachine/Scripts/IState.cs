using System.Collections;

namespace Blues.Core.StateMachine
{
    public interface IState
    {
        void TransitionTo(Transition transition, bool pauseCurrent = false);
        IEnumerator ClearPausedStates();
        IEnumerator ReloadCurrentState();

        // Current canvas sorting order — increments when a state is pushed, decrements when popped
        int CurrentSortingOrder { get; }
    }
}