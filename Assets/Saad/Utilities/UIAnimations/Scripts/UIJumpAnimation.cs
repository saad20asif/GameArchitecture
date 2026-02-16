using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using Blues.Core.PoolSystem;
using Blues.Core.SoundSystem;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class UIJumpAnimation : MonoBehaviour, IUIAnimation
{
    [Header("References")]
    [SerializeField] private RectTransform source;
    [SerializeField] private RectTransform target;
    [SerializeField] private SoundService soundService;

    [Header("Settings")]
    [SerializeField] private float jumpHeight = 200f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private Ease ease = Ease.OutQuad;
    [SerializeField] private bool loop = false;

    [Header("Impact Effect")]
    [SerializeField] private string impactParticleId;         // Pool ID for particle
    [SerializeField] private bool playImpactOnComplete = true;
    [SerializeField] private PoolManagerSO poolManager;
    [SerializeField] private float returnParticlesAfter = 2f;
    [SerializeField] private Vector3 impactOffset;            // particle offset (world space)
    [SerializeField] private Vector3 targetParticlesScale = Vector3.one;

    [Header("Events")]
    [SerializeField] private UnityEvent onJumpComplete;
    public event Action JumpCompleted;

    private Tween _tween;
    private Vector2 _startAnchoredPos;
    private Vector3 _startScale;

    private void Awake()
    {
        if (source == null)
            source = GetComponent<RectTransform>();

        _startAnchoredPos = source.anchoredPosition;
        _startScale = source.localScale;
    }

    [Button("Play Jump")]
    public void Play()
    {
        if (source == null || target == null)
        {
            Debug.LogWarning("Source or Target not assigned!");
            return;
        }

        _tween?.Kill();
        Reset();
        // Store start anchored position
        _startAnchoredPos = source.anchoredPosition;

        // ✅ Use DOJumpAnchorPos for all UI movements (pure anchored space)
        _tween = source
            .DOJumpAnchorPos(target.anchoredPosition, jumpHeight, 1, duration)
            .SetEase(ease)
            .SetLoops(loop ? -1 : 0)
            .OnComplete(OnJumpComplete);
    }

    [Button("Reset")]
    public void Reset()
    {
        _tween?.Kill();
        source.anchoredPosition = _startAnchoredPos;
        source.localScale = _startScale;
    }

    private void OnJumpComplete()
    {
        if (playImpactOnComplete && !string.IsNullOrEmpty(impactParticleId) && poolManager != null)
        {
            // 💥 Only particles use world space position (for VFX placement)
            GameObject particles = poolManager.Get(impactParticleId);
            Vector3 spawnPos = target.position + impactOffset;
            particles.transform.position = spawnPos;
            particles.transform.localScale = targetParticlesScale;
            particles.SetActive(true);

            StartCoroutine(ReturnToPoolAfterDelay(impactParticleId, particles, returnParticlesAfter));
        }

        onJumpComplete?.Invoke();
        JumpCompleted?.Invoke();
        soundService.Play("UIHitImpact");
    }

    private IEnumerator ReturnToPoolAfterDelay(string poolId, GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (go != null)
            poolManager.Release(poolId, go);
    }
}
