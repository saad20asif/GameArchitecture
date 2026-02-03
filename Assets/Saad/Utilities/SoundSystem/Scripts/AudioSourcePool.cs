using UnityEngine;
using UnityEngine.Pool;

namespace THEBADDEST.SoundSystem
{
    public class AudioSourcePool
    {
        private readonly Transform _parent;
        private readonly IObjectPool<AudioSource> _pool;

        public AudioSourcePool(Transform parent, int maxSize = 30)
        {
            _parent = parent;

            _pool = new ObjectPool<AudioSource>(
                createFunc: CreateNewAudioSource,
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: true,
                defaultCapacity: 8,
                maxSize: maxSize
            );
        }

        private AudioSource CreateNewAudioSource()
        {
            var obj = new GameObject("PooledAudioSource", typeof(AudioSource));
            obj.transform.SetParent(_parent, false);

            var source = obj.GetComponent<AudioSource>();
            source.playOnAwake = false;

            // Add once. Never AddComponent at runtime again.
            var autoReturn = obj.AddComponent<AutoReturnToPool>();
            autoReturn.Bind(source);

            return source;
        }

        private void OnTakeFromPool(AudioSource source)
        {
            source.gameObject.SetActive(true);

            // Reset per-take
            source.Stop();
            source.volume = 1f;
            source.pitch = 1f;
            source.loop = false;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = null;
            source.clip = null;

            // Ensure auto-return is disabled until requested.
            var autoReturn = source.GetComponent<AutoReturnToPool>();
            autoReturn.Disable();
        }

        private void OnReturnToPool(AudioSource source)
        {
            if (source == null) return;

            var autoReturn = source.GetComponent<AutoReturnToPool>();
            autoReturn.Disable();

            source.Stop();
            source.clip = null;
            source.outputAudioMixerGroup = null;
            source.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(AudioSource source)
        {
            if (source != null) Object.Destroy(source.gameObject);
        }

        public AudioSource Get() => _pool.Get();

        public void Return(AudioSource source)
        {
            if (source != null) _pool.Release(source);
        }

        /// <summary>
        /// For non-looping clips: returns to pool automatically after playback ends.
        /// Allocation-free (no coroutine).
        /// </summary>
        public void ReturnWhenFinished(AudioSource source)
        {
            if (source == null) return;

            if (!source.isPlaying)
            {
                Return(source);
                return;
            }

            var autoReturn = source.GetComponent<AutoReturnToPool>();
            autoReturn.Enable(this);
        }
    }

    /// <summary>
    /// Allocation-free "return when finished" helper.
    /// Uses Update + DSP end time to avoid coroutine GC.
    /// </summary>
    public sealed class AutoReturnToPool : MonoBehaviour
    {
        private AudioSource _source;
        private AudioSourcePool _pool;
        private bool _armed;
        private double _endDspTime;

        public void Bind(AudioSource source) => _source = source;

        public void Enable(AudioSourcePool pool)
        {
            _pool = pool;
            _armed = true;

            // Predict end time. More reliable than polling with WaitForSeconds.
            if (_source != null && _source.clip != null)
            {
                var pitch = Mathf.Max(0.0001f, _source.pitch);
                _endDspTime = AudioSettings.dspTime + (_source.clip.length / pitch);
            }
            else
            {
                _endDspTime = AudioSettings.dspTime; // return next Update
            }
        }

        public void Disable()
        {
            _armed = false;
            _pool = null;
            _endDspTime = 0;
        }

        private void Update()
        {
            if (!_armed || _pool == null || _source == null) return;

            // End condition: clip ended (or time passed and not playing).
            if (!_source.isPlaying && AudioSettings.dspTime >= _endDspTime)
            {
                var pool = _pool; // local copy
                Disable();
                pool.Return(_source);
            }
        }
    }
}
