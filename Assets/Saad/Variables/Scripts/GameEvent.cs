using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "e_", menuName = "ProjectCore/Events/Game Event - Basic")]
public class GameEvent : ScriptableObject
{
    public delegate void EventHandler();
    public event EventHandler Handler;

    [Button(ButtonSizes.Medium)]
    public void Invoke()
    {
        Handler?.Invoke();
    }
}
