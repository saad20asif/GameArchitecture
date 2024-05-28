using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Transition", menuName ="ProjectCore/State Machine/Transitions/Basic Transition")]
public class Transition : ScriptableObject
{
    public State ToState;
}
