using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class UiAnimationSystem
{
    private CanvasGroup _canvasGroup;
    private RectTransform _uiPanel;
    private Animator _animator;
    private StateAnimationConfig _config;
    private Coroutine _currentAnimationCoroutine;
    private Tween _currentTween;
    private MonoBehaviour _monoBehaviour;
    
    private bool _isAnimating;
    private AnimationPhase _currentPhase;

    public UiAnimationSystem(MonoBehaviour monoBehaviour, CanvasGroup canvasGroup, 
        RectTransform uiPanel, Animator animator, StateAnimationConfig config)
    {
        _monoBehaviour = monoBehaviour;
        _canvasGroup = canvasGroup;
        _uiPanel = uiPanel;
        _animator = animator;
        _config = config;
    }

    public void PlayAnimation(AnimationPhase phase, Action onComplete = null)
    {
        if (_isAnimating)
        {
            ForceCompleteCurrentAnimation();
        }

        _currentPhase = phase;
        _isAnimating = true;
        
        _currentTween?.Kill();
        if (_currentAnimationCoroutine != null)
        {
            _monoBehaviour.StopCoroutine(_currentAnimationCoroutine);
            _currentAnimationCoroutine = null;
        }

        if (_config.UseCustomAnimation && HasCustomAnimation(phase))
        {
            PlayCustomAnimation(GetCustomAnimationClip(phase), () => 
            {
                if (_animator != null)
                {
                    _animator.enabled = false;
                }
                _isAnimating = false;
                onComplete?.Invoke();
            });
        }
        else
        {
            PlayDefaultAnimation(phase, () => 
            {
                _isAnimating = false;
                onComplete?.Invoke();
            });
        }
    }

    public void ForceCompleteCurrentAnimation()
    {
        if (_currentTween != null && _currentTween.IsActive())
        {
            _currentTween.Complete();
            _currentTween.Kill();
        }
        
        if (_currentAnimationCoroutine != null)
        {
            _monoBehaviour.StopCoroutine(_currentAnimationCoroutine);
            _currentAnimationCoroutine = null;
            if (_animator != null)
            {
                _animator.enabled = false;
            }
        }
        
        _isAnimating = false;
    }

    public void ResetToShownState()
    {
        _canvasGroup.alpha = 1f;
        _uiPanel.localScale = Vector3.one;
        _uiPanel.localPosition = new Vector3(0, _uiPanel.localPosition.y, _uiPanel.localPosition.z);
        if (_animator != null)
        {
            _animator.enabled = false;
        }
    }

    private void SetInitialState(AnimationPhase phase)
    {
        DefaultAnimationType type = GetAnimationType(phase);
        Vector3 currentPos = _uiPanel.localPosition;

        // Reset all animation-affecting properties first
        _canvasGroup.alpha = 1f;
        _uiPanel.localScale = Vector3.one;
        _uiPanel.localPosition = new Vector3(0, currentPos.y, currentPos.z);

        switch (type)
        {
            case DefaultAnimationType.Fade:
                _canvasGroup.alpha = phase switch
                {
                    AnimationPhase.Enter => 0f,
                    AnimationPhase.Resume => 0f, // <- start from transparent
                    _ => 1f
                };
                break;

            case DefaultAnimationType.Scale:
                _uiPanel.localScale = phase switch
                {
                    AnimationPhase.Enter => Vector3.zero,
                    AnimationPhase.Resume => Vector3.zero, // <- start from scaled down
                    _ => Vector3.one
                };
                break;

            case DefaultAnimationType.Slide:
                _uiPanel.localPosition = phase switch
                {
                    AnimationPhase.Enter => new Vector3(Screen.width, currentPos.y, currentPos.z),
                    AnimationPhase.Resume => new Vector3(Screen.width, currentPos.y, currentPos.z), // <- offscreen
                    _ => new Vector3(0, currentPos.y, currentPos.z)
                };
                break;
        }
    }



    private bool HasCustomAnimation(AnimationPhase phase)
    {
        return phase switch
        {
            AnimationPhase.Enter => _config.CustomEnterAnimation != null,
            AnimationPhase.Exit => _config.CustomExitAnimation != null,
            AnimationPhase.Pause => _config.CustomPauseAnimation != null,
            AnimationPhase.Resume => _config.CustomResumeAnimation != null,
            _ => false
        };
    }

    private AnimationClip GetCustomAnimationClip(AnimationPhase phase)
    {
        return phase switch
        {
            AnimationPhase.Enter => _config.CustomEnterAnimation,
            AnimationPhase.Exit => _config.CustomExitAnimation,
            AnimationPhase.Pause => _config.CustomPauseAnimation,
            AnimationPhase.Resume => _config.CustomResumeAnimation,
            _ => null
        };
    }

    private void PlayCustomAnimation(AnimationClip clip, Action onComplete)
    {
        if (_animator == null || clip == null) return;

        _animator.enabled = true;
        _animator.Play(clip.name);

        _currentAnimationCoroutine = _monoBehaviour.StartCoroutine(WaitForAnimationComplete(clip.length, onComplete));
    }

    private IEnumerator WaitForAnimationComplete(float duration, Action callback)
    {
        yield return new WaitForSeconds(duration);
        callback?.Invoke();
        _currentAnimationCoroutine = null;
    }

    private void PlayDefaultAnimation(AnimationPhase phase, Action onComplete)
    {
        SetInitialState(phase);
        DefaultAnimationType type = GetAnimationType(phase);
        
        switch (type)
        {
            case DefaultAnimationType.Fade:
                PlayFadeAnimation(phase, onComplete);
                break;
            case DefaultAnimationType.Scale:
                PlayScaleAnimation(phase, onComplete);
                break;
            case DefaultAnimationType.Slide:
                PlaySlideAnimation(phase, onComplete);
                break;
            case DefaultAnimationType.None:
                onComplete?.Invoke();
                break;
        }
    }

    private DefaultAnimationType GetAnimationType(AnimationPhase phase)
    {
        return phase switch
        {
            AnimationPhase.Enter => _config.EnterAnimationType,
            AnimationPhase.Exit => _config.ExitAnimationType,
            AnimationPhase.Pause => _config.PauseAnimationType,
            AnimationPhase.Resume => _config.ResumeAnimationType,
            _ => DefaultAnimationType.None
        };
    }

    private void PlayFadeAnimation(AnimationPhase phase, Action onComplete)
    {
        float targetAlpha = GetTargetAlpha(phase);
        float duration = GetDuration(phase);
        Ease ease = GetEase(phase);

        _currentTween = _canvasGroup.DOFade(targetAlpha, duration)
            .SetEase(ease)
            .OnComplete(() => onComplete?.Invoke());
    }

    private float GetTargetAlpha(AnimationPhase phase)
    {
        return phase switch
        {
            AnimationPhase.Enter => 1f,
            AnimationPhase.Exit => 0f,
            AnimationPhase.Pause => 0f,
            AnimationPhase.Resume => 1f,
            _ => 1f
        };
    }

    private void PlayScaleAnimation(AnimationPhase phase, Action onComplete)
    {
        Vector3 targetScale = GetTargetScale(phase);
        float duration = GetDuration(phase);
        Ease ease = GetEase(phase);

        _currentTween = _uiPanel.DOScale(targetScale, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            });
    }

    private Vector3 GetTargetScale(AnimationPhase phase)
    {
        Debug.Log($"Getting target scale for {phase}");
        return phase switch
        {
            AnimationPhase.Enter => Vector3.one,
            AnimationPhase.Exit => Vector3.zero,
            AnimationPhase.Pause => Vector3.zero,
            AnimationPhase.Resume => Vector3.one,
            _ => Vector3.one
        };
    }

    private void PlaySlideAnimation(AnimationPhase phase, Action onComplete)
    {
        Vector3 targetPos = GetTargetPosition(phase);
        float duration = GetDuration(phase);
        Ease ease = GetEase(phase);

        _currentTween = _uiPanel.DOLocalMove(targetPos, duration)
            .SetEase(ease)
            .OnComplete(() => onComplete?.Invoke());
    }

    private Vector3 GetTargetPosition(AnimationPhase phase)
    {
        return phase switch
        {
            AnimationPhase.Enter => new Vector3(0, _uiPanel.localPosition.y, 0),
            AnimationPhase.Exit => new Vector3(-Screen.width, _uiPanel.localPosition.y, 0),
            AnimationPhase.Pause => new Vector3(Screen.width, _uiPanel.localPosition.y, 0),
            AnimationPhase.Resume => new Vector3(0, _uiPanel.localPosition.y, 0),
            _ => _uiPanel.localPosition
        };
    }

    private float GetDuration(AnimationPhase phase)
    {
        return phase switch
        {
            AnimationPhase.Enter => _config.EnterDuration,
            AnimationPhase.Exit => _config.ExitDuration,
            AnimationPhase.Pause => _config.PauseDuration,
            AnimationPhase.Resume => _config.ResumeDuration,
            _ => 0f
        };
    }

    private Ease GetEase(AnimationPhase phase)
    {
        return phase switch
        {
            AnimationPhase.Enter => _config.EnterEase,
            AnimationPhase.Exit => _config.ExitEase,
            AnimationPhase.Pause => _config.PauseEase,
            AnimationPhase.Resume => _config.ResumeEase,
            _ => Ease.Linear
        };
    }
}