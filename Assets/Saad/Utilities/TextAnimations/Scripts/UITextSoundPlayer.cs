using Blues.Core.SoundSystem;
using Core.UI.TextAnimations;
using UnityEngine;

public class UITextSoundPlayer : MonoBehaviour
{
    [SerializeField] private SoundService soundService;
    [SerializeField] private string soundName = "Perfect";

    private void OnEnable()
    {
        TMPTextAnimator.OnTextAnimationPlayed += PlaySound;
    }

    private void OnDisable()
    {
        TMPTextAnimator.OnTextAnimationPlayed -= PlaySound;
    }

    void PlaySound()
    {
        soundService.Play(soundName);
    }
}