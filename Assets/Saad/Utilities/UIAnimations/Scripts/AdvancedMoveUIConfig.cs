using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "AdvancedMoveUIConfig", menuName = "UI Animations/Advanced Move UI Config")]
public class AdvancedMoveUIConfig : ScriptableObject
{
    [Header("Anticipation Settings")]
    [SerializeField] public float anticipationTime = 0.2f;
    [SerializeField] public Ease anticipationEase = Ease.OutBack;
    
    [Header("Scale Animation")]
    [SerializeField] public bool enableScaleAnticipation = true;
    [SerializeField] public float anticipationScale = 1.3f;
    [SerializeField] public float targetScale = 0.5f;
    [SerializeField] public bool enableSquashStretch = true;
    [SerializeField] public float squashScale = 0.8f;
    [SerializeField] public float stretchScale = 1.5f;

    [Header("Movement Anticipation")]
    [SerializeField] public bool enableReverseAnticipation = true;
    [SerializeField] public float reverseDistance = 50f;
    [SerializeField] public Ease reverseEase = Ease.OutQuad;
    
    [Header("Perpendicular Anticipation")]
    [SerializeField] public bool enablePerpendicularAnticipation = true;
    [SerializeField] public float perpendicularDistance = 30f;
    [SerializeField] public bool slideLeft = false; // false = right, true = left
    [SerializeField] public float perpendicularDuration = 0.15f;
    [SerializeField] public Ease perpendicularEase = Ease.OutQuad;

    [Header("Rotation")]
    [SerializeField] public bool enableRotation = true;
    [SerializeField] public Vector3 rotationAxis = Vector3.forward;
    [SerializeField] public float rotationSpeed = 360f; // degrees per second
    [SerializeField] public bool randomRotationDirection = true;

    [Header("Hit Movement")]
    [SerializeField] public float hitDuration = 0.3f;
    [SerializeField] public Ease hitEase = Ease.InQuad;

    [Header("Impact Effect")]
    [SerializeField] public string impactParticleId;
    [SerializeField] public bool playImpactOnComplete = true;
     [SerializeField] public bool playImpactOnStart = false;
    [SerializeField] public float returnParticlesAfter = 2f;
    [SerializeField] public Vector3 impactOffset = Vector3.zero;
    [SerializeField] public Vector3 particlesRotation = Vector3.zero;
    [SerializeField] public float particlesScale = 1f;
}

