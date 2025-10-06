using UnityEngine;
using System.Collections;
using Blues.Core.UI;
using Sirenix.OdinInspector;

namespace Blues.Core.StateMachine
{
    [CreateAssetMenu(fileName = "Transition", menuName = "ProjectCore/State Machine/Transitions/Basic Transition")]
    public class Transition : ScriptableObject
    {
        public State ToState;

        [EnumToggleButtons]
        [InfoBox("Uses FSM's default policy for the given reason.", "@closePolicy == ClosePolicy.Default")]
        public ClosePolicy closePolicy = ClosePolicy.Default;

        public virtual IEnumerator Execute()
        {
            yield break;
        }
    }
}