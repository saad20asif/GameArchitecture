using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using Blues.Core.PoolSystem;
using Blues.Core.SoundSystem;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class AdvancedMoveUI : MonoBehaviour, IUIAnimation
{
    [Header("References")]
    [SerializeField] private RectTransform source;
    [SerializeField] private RectTransform target;
    [SerializeField] private AdvancedMoveUIConfig config;
    [SerializeField] private PoolManagerSO poolManager;

    [Header("Events")]
    [SerializeField] private UnityEvent onHitComplete;
    [SerializeField] private SoundService soundService;

    public event Action HitCompleted; 
    public event Action AnticipationCompleted;


    // Private fields
    private Sequence _mainSequence;
    private Tween _rotationTween;
    private Vector3 _startScale;
    private Vector2 _startPos;
    private Vector3 _startRotation;

    private void Awake()
    {
        InitializeReferences();
        ValidateConfiguration();
        if (source == null)
            source = GetComponent<RectTransform>();

        _startScale = source.localScale;
        _startPos = source.anchoredPosition;
        _startRotation = source.localEulerAngles;
    }
    /*public void RecalculateStartState()
    {
        if (source == null)
            source = GetComponent<RectTransform>();

        _startScale = source.localScale;
        _startPos = source.anchoredPosition;
        _startRotation = source.localEulerAngles;
    }*/

    private IEnumerator Start()
    {
        // Wait one frame for UI layout rebuild before caching
        yield return null;

        if (source == null) source = GetComponent<RectTransform>();

        _startScale = source.localScale;
        _startPos = source.anchoredPosition;
        _startRotation = source.localEulerAngles;

        poolManager?.Initialize();
    }

    private void InitializeReferences()
    {
        if (source == null)
            source = GetComponent<RectTransform>();

        if (config == null)
        {
            Debug.LogError($"AdvancedMoveUIConfig is not assigned on {gameObject.name}!");
            return;
        }
    }

    private void ValidateConfiguration()
    {
        if (config == null) return;

        if (config.anticipationTime <= 0) config.anticipationTime = 0.2f;
        if (config.hitDuration <= 0) config.hitDuration = 0.3f;
        if (config.perpendicularDuration <= 0) config.perpendicularDuration = 0.15f;
        if (config.returnParticlesAfter <= 0) config.returnParticlesAfter = 2f;
    }

    [Button("Play Advanced Move")]
    public void Play()
    {
        if (!ValidateSetup()) return;

        Reset();
        _mainSequence = DOTween.Sequence();
        _mainSequence.SetLink(this.gameObject);
        // Pre Animation Phase 
        PlayParticlesOnStart();
        // Phase 1: anticipation
        CreateAnticipationPhase();

        // Phase 2: perpendicular anticipation
        if (config.enablePerpendicularAnticipation)
            CreatePerpendicularAnticipationPhase();

        // Phase 3: action to target
        CreateActionPhase();

        // Phase 4: rotation
        if (config.enableRotation)
            CreateRotationPhase();

        _mainSequence.OnComplete(OnHitComplete);
    }

    [Button("Reset")]
    public void Reset()
    {
        KillAllTweens();
        RestoreInitialValues();
    }

    private bool ValidateSetup()
    {
        if (source == null || target == null)
        {
            Debug.LogWarning($"Source or Target not assigned on {gameObject.name}!");
            return false;
        }

        if (config == null)
        {
            Debug.LogError($"AdvancedMoveUIConfig is not assigned on {gameObject.name}!");
            return false;
        }

        return true;
    }

    private void KillAllTweens()
    {
        _mainSequence?.Kill();
        _rotationTween?.Kill();
        source.DOKill();
        _mainSequence = null;
        _rotationTween = null;
    }

    private void RestoreInitialValues()
    {
        if (source == null) return;

        source.localScale = _startScale;
        source.anchoredPosition = _startPos;
        source.localEulerAngles = _startRotation;
    }

    private void CreateAnticipationPhase()
    {
        Sequence anticipationSeq = DOTween.Sequence();

        // Scale anticipation
        if (config.enableScaleAnticipation)
        {
            if (config.enableSquashStretch)
            {
                anticipationSeq.Append(source.DOScale(_startScale * config.squashScale, config.anticipationTime * 0.3f).SetEase(Ease.OutQuad));
                anticipationSeq.Append(source.DOScale(_startScale * config.anticipationScale, config.anticipationTime * 0.7f).SetEase(config.anticipationEase));
            }
            else
            {
                anticipationSeq.Append(source.DOScale(_startScale * config.anticipationScale, config.anticipationTime).SetEase(config.anticipationEase));
            }
        }

        // Reverse anticipation movement
        if (config.enableReverseAnticipation)
        {
            Vector2 targetPos = GetTargetAnchoredPosition();
            Vector2 directionToTarget = (targetPos - _startPos).normalized;
            Vector2 reversePoint = _startPos - directionToTarget * config.reverseDistance;
            anticipationSeq.Join(source.DOAnchorPos(reversePoint, config.anticipationTime).SetEase(config.reverseEase));
        }

        _mainSequence.Append(anticipationSeq);
        _mainSequence.AppendCallback(() => AnticipationCompleted?.Invoke());
    }

    private void CreatePerpendicularAnticipationPhase()
    {
        Vector2 targetPos = GetTargetAnchoredPosition();
        Vector2 currentPos = GetCurrentAnticipationPosition();
        Vector2 directionToTarget = (targetPos - currentPos).normalized;

        // Perpendicular vector
        Vector3 perpendicular = Vector3.Cross(directionToTarget, Vector3.forward);
        if (config.slideLeft) perpendicular = -perpendicular;

        Vector2 perpendicularPoint = currentPos + (Vector2)(perpendicular * config.perpendicularDistance);
        _mainSequence.Append(source.DOAnchorPos(perpendicularPoint, config.perpendicularDuration).SetEase(config.perpendicularEase));
    }

    private void CreateActionPhase()
    {
        Vector2 startPos = GetActionStartPosition();
        Vector2 targetPos = GetTargetAnchoredPosition();

        // Smooth path (optional mid curve)
        Vector2 midPoint = Vector2.Lerp(startPos, targetPos, 0.4f);

        Sequence actionSeq = DOTween.Sequence();
        actionSeq.Append(source.DOAnchorPos(targetPos, config.hitDuration).SetEase(config.hitEase));

        if (config.enableScaleAnticipation)
        {
            actionSeq.Join(source.DOScale(_startScale * config.targetScale, config.hitDuration).SetEase(config.hitEase));
        }


        _mainSequence.Append(actionSeq);
    }

    private void CreateRotationPhase()
    {
        float totalRotationTime = config.anticipationTime +
                                 (config.enablePerpendicularAnticipation ? config.perpendicularDuration : 0f) +
                                 config.hitDuration;

        Vector3 rotationDirection = config.randomRotationDirection
            ? (UnityEngine.Random.Range(0, 2) == 0 ? config.rotationAxis : -config.rotationAxis)
            : config.rotationAxis;

        float totalRotation = config.rotationSpeed * totalRotationTime;

        _rotationTween = source.DOLocalRotate(_startRotation + rotationDirection * totalRotation, totalRotationTime, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear);
    }

    private Vector2 GetCurrentAnticipationPosition()
    {
        if (config.enableReverseAnticipation)
        {
            Vector2 targetPos = GetTargetAnchoredPosition();
            Vector2 directionToTarget = (targetPos - _startPos).normalized;
            return _startPos - directionToTarget * config.reverseDistance;
        }
        return _startPos;
    }

    private Vector2 GetActionStartPosition()
    {
        Vector2 pos = GetCurrentAnticipationPosition();

        if (config.enablePerpendicularAnticipation)
        {
            Vector2 targetPos = GetTargetAnchoredPosition();
            Vector2 directionToTarget = (targetPos - _startPos).normalized;
            Vector3 perpendicular = Vector3.Cross(directionToTarget, Vector3.forward);
            if (config.slideLeft) perpendicular = -perpendicular;
            pos += (Vector2)(perpendicular * config.perpendicularDistance);
        }

        return pos;
    }

    /// <summary>
    /// Returns target position converted to source's parent space if needed.
    /// </summary>
    private Vector2 GetTargetAnchoredPosition()
    {
        if (target == null || source == null)
            return Vector2.zero;

        // If both share same parent, anchoredPosition works directly
        if (target.parent == source.parent)
            return target.anchoredPosition;

        // Otherwise convert from world to local coordinates in source's parent space
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            source.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(null, target.position),
            null,
            out localPoint
        );
        return localPoint;
    }

    private void OnHitComplete()
    {
        // Optional particles
        if (config.playImpactOnComplete && !string.IsNullOrEmpty(config.impactParticleId) && poolManager != null)
        {
            GameObject particles = poolManager.Get(config.impactParticleId);
            Vector3 spawnPos = target.transform.position + config.impactOffset;
            particles.transform.position = spawnPos;
            particles.transform.localScale = new Vector3(config.particlesScale, config.particlesScale, config.particlesScale);
            particles.transform.localEulerAngles = config.particlesRotation;
            particles.SetActive(true);
            poolManager.ReleaseAfterDelay(config.impactParticleId, particles, config.returnParticlesAfter);

        }

        onHitComplete?.Invoke();
        HitCompleted?.Invoke();
        soundService?.Play("UIHitImpact");
    }
    private void PlayParticlesOnStart()
    {
        if (config.playImpactOnStart && !string.IsNullOrEmpty(config.impactParticleId) && poolManager != null)
        {
            GameObject particles = poolManager.Get(config.impactParticleId);
            Vector3 spawnPos = source.transform.position;
            particles.transform.position = spawnPos;
            particles.transform.localScale = new Vector3(config.particlesScale, config.particlesScale, config.particlesScale);
            particles.SetActive(true);
            poolManager.ReleaseAfterDelay(config.impactParticleId, particles, config.returnParticlesAfter);
        }
    }
    private IEnumerator ReturnToPoolAfterDelay(string poolId, GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (go != null)
            poolManager.Release(poolId, go);

        if (go != null)
            go.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        KillAllTweens();
    }

    private void OnDisable()
    {
        KillAllTweens();
    }

    public void SetSource(RectTransform source)
    {
        this.source = source;
    }

    public void SetTarget(RectTransform target )
    {
        this.target = target ;
    }
}
