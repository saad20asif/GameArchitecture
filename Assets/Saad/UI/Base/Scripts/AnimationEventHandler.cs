using System;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    public event Action OnAnimationComplete;

    // Called from Animation Events
    public void AnimationCompleted()
    {
        OnAnimationComplete?.Invoke();
    }
}