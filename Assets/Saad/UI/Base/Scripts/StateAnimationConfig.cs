using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "UI/Animation Config")]
public class StateAnimationConfig : ScriptableObject
{
    [Header("Custom Animations")]
    public bool UseCustomAnimation = false;
    public AnimationClip CustomEnterAnimation;
    public AnimationClip CustomExitAnimation;
    public AnimationClip CustomPauseAnimation;
    public AnimationClip CustomResumeAnimation;
    
    [Header("Default Enter/Exit")]
    public DefaultAnimationType EnterAnimationType = DefaultAnimationType.Fade;
    public DefaultAnimationType ExitAnimationType = DefaultAnimationType.Fade;
    public Ease EnterEase = Ease.OutBack;
    public Ease ExitEase = Ease.InBack;
    public float EnterDuration = 0.3f;
    public float ExitDuration = 0.3f;
    
    [Header("Pause/Resume")]
    public DefaultAnimationType PauseAnimationType = DefaultAnimationType.None;
    public DefaultAnimationType ResumeAnimationType = DefaultAnimationType.None;
    public Ease PauseEase = Ease.Linear;
    public Ease ResumeEase = Ease.Linear;
    public float PauseDuration = 0.2f;
    public float ResumeDuration = 0.2f;
}

public enum DefaultAnimationType
{
    Fade,
    Scale,
    Slide,
    None
}

public enum AnimationPhase
{
    Enter,
    Exit,
    Pause,
    Resume
}