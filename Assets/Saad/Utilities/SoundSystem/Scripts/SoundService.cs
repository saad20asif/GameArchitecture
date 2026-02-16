using System;
using System.Collections;
using System.Collections.Generic;
using Blues.Core.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using THEBADDEST.Coroutines;

namespace Blues.Core.SoundSystem
{
    [CreateAssetMenu(fileName = "SoundService", menuName = "THEBADDEST/SoundSystem/SoundService", order = 0)]
    public class SoundService : ScriptableObject, IList<Sound>
    {
        [SerializeField] private List<Sound> sounds = new();
        [SerializeField] private int maxConcurrentSounds = 30;
        [SerializeField] private DBBoolWithEvent Sound;
        [SerializeField] private SoundSettings settings;

        private Transform parent;
        private AudioSourcePool audioSourcePool;
        private Dictionary<string, Sound> soundDictionary;
        private readonly HashSet<Sound> playingLoopSounds = new();
        private readonly Dictionary<string, AudioSource> activeLoopSources = new();
        private readonly Dictionary<string, Coroutine> activeFades = new();

        private bool isInitialized;

        public int Count => sounds.Count;
        public bool IsReadOnly => false;

        private void OnEnable()
        {
            isInitialized = false;
        }

        public void Initialize()
        {
            if (isInitialized) return;

            parent = new GameObject("GameSounds").transform;
            DontDestroyOnLoad(parent.gameObject);

            audioSourcePool = new AudioSourcePool(parent, maxConcurrentSounds);
            soundDictionary = new Dictionary<string, Sound>(sounds.Count);

            // IMPORTANT: set initialized before PlayOnAwake.
            isInitialized = true;

            foreach (var s in sounds)
            {
                if (s == null || string.IsNullOrEmpty(s.SoundName))
                    continue;

                if (soundDictionary.ContainsKey(s.SoundName))
                    continue;

                soundDictionary.Add(s.SoundName, s);

                if (s.PlayOnAwake)
                    PlaySoundInternal(s);
            }
        }

        public void Shutdown()
        {
            StopAllSounds();

            if (parent != null)
                Destroy(parent.gameObject);

            isInitialized = false;
        }
        
        [Button]
        public void Play(string soundName, float delay = 0f)
        {
            if (!isInitialized) return;
            if (Sound != null && !Sound.GetValue()) return;

            var sound = Find(soundName);
            if (sound == null) return;

            if (delay > 0f)
                CoroutineHandler.StartStaticCoroutine(PlayWithDelay(sound, delay));
            else
                PlaySoundInternal(sound);
        }

        public void Play(string soundName, Vector3 position)
        {
            if (!isInitialized) return;
            if (Sound != null && !Sound.GetValue()) return;

            var sound = Find(soundName);
            if (sound == null) return;

            PlayAtPositionInternal(sound, position);
        }

        private IEnumerator PlayWithDelay(Sound sound, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlaySoundInternal(sound);
        }

        private void PlaySoundInternal(Sound sound)
        {
            if (sound == null) return;
            if (Sound != null && !Sound.GetValue()) return;

            // Loop sounds: keep exactly one active source per sound name
            if (sound.Loop && activeLoopSources.TryGetValue(sound.SoundName, out var existing) && existing != null)
            {
                if (existing.isPlaying) return; // already playing, ignore
                audioSourcePool.Return(existing);
                activeLoopSources.Remove(sound.SoundName);
            }

            var source = audioSourcePool.Get();
            if (source == null) return;

            ConfigureAudioSource(sound, source);
            if (source.clip == null)
            {
                audioSourcePool.Return(source);
                return;
            }

            source.Play();

            if (sound.Loop)
            {
                sound.Source = source;
                playingLoopSounds.Add(sound);
                activeLoopSources[sound.SoundName] = source;
            }
            else
            {
                audioSourcePool.ReturnWhenFinished(source);
            }
        }

        private void PlayAtPositionInternal(Sound sound, Vector3 position)
        {
            if (sound == null) return;

            var source = audioSourcePool.Get();
            if (source == null) return;

            ConfigureAudioSource(sound, source);
            if (source.clip == null)
            {
                audioSourcePool.Return(source);
                return;
            }

            source.transform.position = position;
            source.spatialBlend = 1f;
            source.Play();

            if (sound.Loop)
            {
                sound.Source = source;
                playingLoopSounds.Add(sound);
                activeLoopSources[sound.SoundName] = source;
            }
            else
            {
                audioSourcePool.ReturnWhenFinished(source);
            }
        }

        private void ConfigureAudioSource(Sound sound, AudioSource source)
        {
            source.gameObject.name = sound.SoundName;

            AudioClip clip = null;
            if (sound.PlayRandomClip && sound.AudioClips != null && sound.AudioClips.Length > 0)
                clip = sound.AudioClips[UnityEngine.Random.Range(0, sound.AudioClips.Length)];
            else
                clip = sound.AudioClip;

            source.clip = clip;

            source.volume = sound.Volume;
            source.pitch = sound.UseRandomPitch ? UnityEngine.Random.Range(sound.MinPitch, sound.MaxPitch) : sound.Pitch;
            source.loop = sound.Loop;
            source.outputAudioMixerGroup = settings != null ? settings.GetAudioMixerGroup(sound.Type) : null;
            source.spatialBlend = 0f;
        }

        public void FadeIn(string soundName, float duration, float targetVolume = 1f, Action onComplete = null)
        {
            if (!isInitialized) return;

            var sound = Find(soundName);
            if (sound == null) return;

            // Ensure we have a source (especially for loop sounds)
            if (!activeLoopSources.TryGetValue(soundName, out var source) || source == null)
            {
                source = audioSourcePool.Get();
                if (source == null) return;

                ConfigureAudioSource(sound, source);
                if (source.clip == null)
                {
                    audioSourcePool.Return(source);
                    return;
                }

                sound.Source = source;
                if (sound.Loop)
                {
                    playingLoopSounds.Add(sound);
                    activeLoopSources[soundName] = source;
                }
                else
                {
                    audioSourcePool.ReturnWhenFinished(source);
                }

                source.volume = 0f;
                source.Play();
            }

            if (activeFades.TryGetValue(soundName, out var existing))
                CoroutineHandler.StopStaticCoroutine(existing);

            activeFades[soundName] = CoroutineHandler.StartStaticCoroutine(
                FadeCoroutine(source, source.volume, targetVolume, duration, onComplete));
        }

        [Button]
        public void FadeOut(string soundName, float duration, Action onComplete = null)
        {
            if (!isInitialized) return;

            if (activeFades.TryGetValue(soundName, out var existing))
                CoroutineHandler.StopStaticCoroutine(existing);

            if (!activeLoopSources.TryGetValue(soundName, out var source) || source == null)
                return;

            activeFades[soundName] = CoroutineHandler.StartStaticCoroutine(
                FadeCoroutine(source, source.volume, 0f, duration, () =>
                {
                    source.Stop();
                    audioSourcePool.Return(source);
                    activeLoopSources.Remove(soundName);

                    var s = Find(soundName);
                    if (s != null)
                    {
                        playingLoopSounds.Remove(s);
                        s.Source = null;
                    }

                    onComplete?.Invoke();
                }));
        }

        private IEnumerator FadeCoroutine(AudioSource source, float start, float end, float duration, Action onComplete)
        {
            float t = 0f;
            while (t < duration && source != null && source.gameObject.activeInHierarchy)
            {
                t += Time.deltaTime;
                source.volume = Mathf.Lerp(start, end, t / duration);
                yield return null;
            }

            if (source != null && source.gameObject.activeInHierarchy)
                source.volume = end;

            onComplete?.Invoke();
        }

        [Button]
        public void Stop(string soundName)
        {
            if (!isInitialized) return;

            if (activeFades.TryGetValue(soundName, out var fade))
            {
                CoroutineHandler.StopStaticCoroutine(fade);
                activeFades.Remove(soundName);
            }

            if (activeLoopSources.TryGetValue(soundName, out var source) && source != null)
            {
                source.Stop();
                audioSourcePool.Return(source);
                activeLoopSources.Remove(soundName);
            }

            var s = Find(soundName);
            if (s != null)
            {
                playingLoopSounds.Remove(s);
                s.Source = null;
            }
        }

        public void StopAllSounds()
        {
            if (!isInitialized) return;

            foreach (var kv in activeLoopSources)
            {
                if (kv.Value == null) continue;
                kv.Value.Stop();
                audioSourcePool.Return(kv.Value);
            }

            activeLoopSources.Clear();
            playingLoopSounds.Clear();

            foreach (var k in new List<string>(activeFades.Keys))
            {
                CoroutineHandler.StopStaticCoroutine(activeFades[k]);
            }
            activeFades.Clear();
        }

        public void SetVolume(string soundName, float volume)
        {
            var sound = Find(soundName);
            if (sound == null) return;
            sound.Volume = volume;

            // If looping source is active, apply immediately
            if (activeLoopSources.TryGetValue(soundName, out var src) && src != null)
                src.volume = sound.Volume;
        }

        public void SetMute(bool value)
        {
            if (settings != null) settings.IsMute = value;
        }

        public Sound Find(string soundName)
        {
            if (string.IsNullOrEmpty(soundName)) return null;
            if (soundDictionary == null) return null;
            return soundDictionary.TryGetValue(soundName, out var sound) ? sound : null;
        }

        #region IList Implementation
        public Sound this[int index] { get => sounds[index]; set => sounds[index] = value; }

        public void Add(Sound item)
        {
            if (item == null || string.IsNullOrEmpty(item.SoundName)) return;

            sounds.Add(item);

            if (isInitialized && soundDictionary != null && !soundDictionary.ContainsKey(item.SoundName))
                soundDictionary.Add(item.SoundName, item);
        }

        public void Clear()
        {
            StopAllSounds();
            sounds.Clear();
            soundDictionary?.Clear();
        }

        public bool Contains(Sound item) => sounds.Contains(item);
        public void CopyTo(Sound[] array, int arrayIndex) => sounds.CopyTo(array, arrayIndex);
        public int IndexOf(Sound item) => sounds.IndexOf(item);
        public void Insert(int index, Sound item) => sounds.Insert(index, item);

        public bool Remove(Sound item)
        {
            if (item == null) return false;
            Stop(item.SoundName);
            soundDictionary?.Remove(item.SoundName);
            return sounds.Remove(item);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= sounds.Count) return;
            Stop(sounds[index].SoundName);
            soundDictionary?.Remove(sounds[index].SoundName);
            sounds.RemoveAt(index);
        }

        public IEnumerator<Sound> GetEnumerator() => sounds.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
