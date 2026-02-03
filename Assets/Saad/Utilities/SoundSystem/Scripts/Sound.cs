using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Blues.Core.SoundSystem
{
    [Serializable]
    [InlineProperty]
    public class Sound
    {
        [SerializeField]
        [LabelWidth(100)]
        private string soundName;
        public string SoundName { get => soundName; set => soundName = value; }

        [Space(5)]
        [SerializeField]
        private SoundType type = SoundType.SFX;
        public SoundType Type { get => type; set => type = value; }

        [Space(5)]
        [SerializeField]
        private AudioClip audioClip;
        public AudioClip AudioClip { get => audioClip; set => audioClip = value; }

        [SerializeField]
        [ShowIf("playRandomClip")]
        private AudioClip[] audioClips;
        public AudioClip[] AudioClips { get => audioClips; set => audioClips = value; }

        [Space(5)]
        [SerializeField]
        private bool loop = false;
        public bool Loop { get => loop; set => loop = value; }

        [SerializeField]
        private bool playRandomClip = false;
        public bool PlayRandomClip { get => playRandomClip; set => playRandomClip = value; }

        [SerializeField]
        private bool playOnAwake = false;
        public bool PlayOnAwake { get => playOnAwake; set => playOnAwake = value; }

        [Space(5)]
        [SerializeField, Range(0, 1)]
        private float volume = 1;
        public float Volume { get => volume; set => volume = Mathf.Clamp01(value); }

        [SerializeField, Range(0.1f, 3)]
        private float pitch = 1;
        public float Pitch { get => pitch; set => pitch = Mathf.Clamp(value, 0.1f, 3); }

        [Space(5)]
        [SerializeField]
        private bool useRandomPitch;
        public bool UseRandomPitch { get => useRandomPitch; set => useRandomPitch = value; }

        [ShowIf("useRandomPitch")]
        [SerializeField]
        private float minPitch = 0.95f;
        public float MinPitch { get => minPitch; set => minPitch = value; }

        [ShowIf("useRandomPitch")]
        [SerializeField]
        private float maxPitch = 1.05f;
        public float MaxPitch { get => maxPitch; set => maxPitch = value; }

        [HideInInspector] private AudioSource source;
        public AudioSource Source { get => source; set => source = value; }

        [HideInInspector] public bool IsFadingOut;
    }

    public enum SoundType
    {
        SFX,
        UI,
        Music
    }
}
