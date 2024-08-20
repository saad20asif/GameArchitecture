using System.Collections;
using UnityEngine;
namespace ProjectCore.StateMachine
{
    [CreateAssetMenu(fileName = "Transition", menuName = "ProjectCore/State Machine/Transitions/Basic Transition")]
    public class Transition : ScriptableObject
    {
        public State ToState;
        public virtual IEnumerator Execute()
        {
            Debug.LogError(name + " transition executed! ");
            yield break;
        }
    }
}
