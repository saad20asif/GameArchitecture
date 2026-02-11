using System;
using TMPro;
using UnityEngine;
using System.Collections;

namespace Core.UI.TextAnimations
{
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    using Unity.VisualScripting;
#endif

    [ExecuteInEditMode]
    [RequireComponent(typeof(TMP_Text))]
    public class TMPTextAnimator : MonoBehaviour, IPlay
    {
        // -------------------- CONFIG --------------------

        [Header("Animation Config")]
        [SerializeField] private TMPTextAnimationConfig config;
        
        public static event Action OnTextAnimationPlayed;
        // -------------------- INTERNAL STATE --------------------

        TMP_Text _text;
        TMP_TextInfo _info;

        // Final base pose (after curvature)
        Vector3[][] _baseVerts;

        Coroutine _routine;

        bool _isDirty = false;
        bool _isAnimating = false;
        int _activeAnims = 0;

        float _prevCurveHeight = 0f;

        // -------------------- LIFECYCLE --------------------

        void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                ResetAnimation();
                StartCoroutine(PlayDelayed());
            }
        }

#if ODIN_INSPECTOR
        [Button("Play")]
#endif
        public void Play()
        {
            ResetAnimation();
            _routine = StartCoroutine(Animate());
            OnTextAnimationPlayed?.Invoke();

        }

        IEnumerator PlayDelayed()
        {
            // Allow TMP + layout rebuilds to finish
            yield return null;
            yield return new WaitForEndOfFrame();
            Play();
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                bool needUpdate = _text.havePropertiesChanged;

                float currentCurveHeight = config ? config.curveHeight : 0f;
                if (Mathf.Abs(currentCurveHeight - _prevCurveHeight) > 0.001f)
                {
                    needUpdate = true;
                    _prevCurveHeight = currentCurveHeight;
                }

                if (needUpdate)
                {
                    _text.ForceMeshUpdate();
                    _info = _text.textInfo;

                    if (config != null && Mathf.Abs(config.curveHeight) > 0.001f)
                        ApplyCurvature();

                    _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                }
            }
            else if (_isAnimating && _isDirty)
            {
                _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                _isDirty = false;
            }
        }

        private void ResetAnimation()
        {
            StopAllCoroutines();

            _isAnimating = false;
            _activeAnims = 0;

            if (_text == null) return;

            _text.ForceMeshUpdate();
            _info = _text.textInfo;

            if (config != null && Mathf.Abs(config.curveHeight) > 0.001f)
                ApplyCurvature();

            CacheBaseVertices();
            CollapseAllCharacters();

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }

        // -------------------- MAIN FLOW --------------------

        IEnumerator Animate()
        {
            if (config == null)
            {
                Debug.LogWarning("TMPTextAnimator: No config assigned.", this);
                yield break;
            }

            _isAnimating = true;
            _activeAnims = 0;

            for (int c = 0; c < _info.characterCount; c++)
            {
                if (!_info.characterInfo[c].isVisible) continue;

                _activeAnims++;
                StartCoroutine(AnimateCharacter(c));
                yield return new WaitForSeconds(config.characterDelay);
            }
        }

        // -------------------- BASE VERTEX SETUP --------------------

        void CacheBaseVertices()
        {
            _baseVerts = new Vector3[_info.meshInfo.Length][];
            for (int i = 0; i < _info.meshInfo.Length; i++)
                _baseVerts[i] = (Vector3[])_info.meshInfo[i].vertices.Clone();
        }

        void CollapseAllCharacters()
        {
            for (int c = 0; c < _info.characterCount; c++)
            {
                if (!_info.characterInfo[c].isVisible) continue;

                var ch = _info.characterInfo[c];
                int m = ch.materialReferenceIndex;
                int v = ch.vertexIndex;

                Vector3 center = (_baseVerts[m][v] + _baseVerts[m][v + 2]) * 0.5f;
                Vector3[] verts = _info.meshInfo[m].vertices;

                for (int i = 0; i < 4; i++)
                    verts[v + i] = center;
            }
        }

        // -------------------- CHARACTER ANIMATION --------------------

        IEnumerator AnimateCharacter(int index)
        {
            var ch = _info.characterInfo[index];
            int m = ch.materialReferenceIndex;
            int v = ch.vertexIndex;

            Vector3[] verts = _info.meshInfo[m].vertices;
            Vector3 center = (_baseVerts[m][v] + _baseVerts[m][v + 2]) * 0.5f;

            float upTime = config.totalDuration * 0.6f;
            float settleTime = config.totalDuration - upTime;

            // ---- SCALE UP ----
            float t = 0f;
            while (t < upTime)
            {
                float eased = config.easeCurve.Evaluate(t / upTime);
                float scale = Mathf.LerpUnclamped(0f, config.overshootScale, eased);

                ApplyScale(m, v, center, scale, verts);
                t += Time.deltaTime;
                yield return null;
            }

            // ---- SETTLE ----
            t = 0f;
            while (t < settleTime)
            {
                float eased = config.easeCurve.Evaluate(t / settleTime);
                float scale = Mathf.LerpUnclamped(config.overshootScale, 1f, eased);

                ApplyScale(m, v, center, scale, verts);
                t += Time.deltaTime;
                yield return null;
            }

            // Ensure final
            ApplyScale(m, v, center, 1f, verts);

            _activeAnims--;
            if (_activeAnims == 0)
                _isAnimating = false;
        }

        void ApplyScale(int m, int v, Vector3 center, float scale, Vector3[] verts)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = _baseVerts[m][v + i];
                pos -= center;
                pos *= scale;
                pos += center;
                verts[v + i] = pos;
            }

            _isDirty = true;
        }

        // -------------------- CURVATURE --------------------

        void ApplyCurvature()
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            for (int c = 0; c < _info.characterCount; c++)
            {
                if (!_info.characterInfo[c].isVisible) continue;

                minX = Mathf.Min(minX, _info.characterInfo[c].bottomLeft.x);
                maxX = Mathf.Max(maxX, _info.characterInfo[c].topRight.x);
            }

            float width = maxX - minX;
            float centerX = (minX + maxX) * 0.5f;

            if (width <= 0f) return;

            float h = Mathf.Abs(config.curveHeight);
            float L = width * 0.5f;
            float radius = (L * L + h * h) / (2f * h);
            float sign = Mathf.Sign(config.curveHeight);

            for (int c = 0; c < _info.characterCount; c++)
            {
                var ch = _info.characterInfo[c];
                if (!ch.isVisible) continue;

                int m = ch.materialReferenceIndex;
                int v = ch.vertexIndex;
                Vector3[] verts = _info.meshInfo[m].vertices;

                Vector3 charCenter = (verts[v] + verts[v + 2]) * 0.5f;
                float d = charCenter.x - centerX;

                float sqrtTerm = Mathf.Sqrt(Mathf.Max(0f, radius * radius - d * d));
                float offsetY = sign * (h - (radius - sqrtTerm));

                for (int i = 0; i < 4; i++)
                    verts[v + i].y += offsetY;

                float slope = sign * (-d / sqrtTerm);
                float angleRad = Mathf.Atan(slope);

                charCenter = (verts[v] + verts[v + 2]) * 0.5f;

                float cosA = Mathf.Cos(angleRad);
                float sinA = Mathf.Sin(angleRad);

                for (int i = 0; i < 4; i++)
                {
                    Vector3 pos = verts[v + i];
                    pos -= charCenter;

                    float xNew = pos.x * cosA - pos.y * sinA;
                    float yNew = pos.x * sinA + pos.y * cosA;

                    verts[v + i] = new Vector3(xNew, yNew, pos.z) + charCenter;
                }
            }
        }
    }
}