using System;
using UnityEngine;
using UnityEngine.Audio;

using Core;

namespace Audio
{

    public class AudioManager : MonoBehaviour
    {
        // TODO: MAKE SINGLETON

        [SerializeField] AudioMixerGroup musicMix;
        [SerializeField] AudioMixerGroup soundFXMix;

        [SerializeField] Sound[] sounds;

        [Tooltip("Sound that should be played when a sound is not found")]
        [SerializeField] Sound nullSound;

        [SerializeField] float musicTrackFadeInTime = 0.75f;
        [SerializeField] float musicTrackFadeOutTime = 0.15f;


        static AudioManager _current;
        public static AudioManager current => _current;

        public static void Cleanup() {
            Utils.CleanupSingleton<AudioManager>(_current);
        }

        void Awake()
        {
            _current = Utils.ManageSingleton(_current, this);

            // set up audio sources for all sounds
            foreach (Sound s in sounds)
            {
                s.Init(gameObject, soundFXMix);
            }

            // // set up audio sources for all music tracks
            // foreach (Sound s in musicTracks)
            // {
            //     s.Init(gameObject, musicMix);
            // }

            // set up audio source for null sound
            nullSound.Init(gameObject, soundFXMix);
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        Sound FindSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("AudioManager: Sound \"" + name + "\" not found");
            return nullSound;
        }
        return s;
    }
    }
}

