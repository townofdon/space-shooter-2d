using System;
using UnityEngine;
using UnityEngine.Audio;

using Core;

namespace Audio
{

    public class AudioManager : MonoBehaviour {
        [SerializeField] bool debugSounds;
        [SerializeField] bool debugTracks;

        [SerializeField] AudioMixerGroup musicMix;
        [SerializeField] AudioMixerGroup soundFXMix;

        [SerializeField] Sound[] sounds;
        [SerializeField] Track[] tracks;

        [Tooltip("Sound that should be played when a sound is not found")]
        [SerializeField] Sound nullSound;

        // state
        Track currentTrack;
        double cue = 0.0;

        static AudioManager _current;
        public static AudioManager current => _current;

        public static void Cleanup() {
            Utils.CleanupSingleton<AudioManager>(_current);
        }

        public void PlaySound(string name) {
            Sound sound = FindSound(name);
            if (sound != null) sound.Play();
        }

        public void PlayTrack(string name) {
            Track track = FindTrack(name);
            if (track == null) return;
            if (currentTrack != null && currentTrack.name != track.name) currentTrack.Stop();
            currentTrack = track;
            track.Play();
        }

        public void CueTrack(string name) {
            Track track = FindTrack(name);
            if (track == null) return;
            if (currentTrack == null) {
                currentTrack = track;
                track.CueStart(0.0);
                return;
            }
            if (currentTrack.isPlaying && currentTrack.name == track.name) return;
            cue = currentTrack.CueStop();
            track.CueStart(cue);
            currentTrack = track;
        }

        public void StopTrack() {
            if (currentTrack == null) return;
            if (!currentTrack.isPlaying) return;
            currentTrack.Stop();
            currentTrack = null;
        }

        void Awake()
        {
            _current = Utils.ManageSingleton(_current, this);

            // set up audio sources for all sounds
            foreach (Sound s in sounds)
            {
                s.Init(this, soundFXMix);
            }

            // // set up audio sources for all music tracks
            foreach (Track s in tracks) {
                s.Init(this, musicMix);
            }

            // set up audio source for null sound
            nullSound.Init(this, soundFXMix);
        }

        Sound FindSound(string name) {
            Sound s = Array.Find(sounds, sound => sound.name == name);
            if (s == null) {
                Debug.LogWarning("AudioManager: Sound \"" + name + "\" not found");
                return nullSound;
            }
            return s;
        }

        Track FindTrack(string name) {
            Track s = Array.Find(tracks, track => track.name == name);
            if (s == null) Debug.LogWarning("AudioManager: Track \"" + name + "\" not found");
            return s;
        }

        void OnGUI() {
            if (debugSounds) {
                GUILayout.TextField("SOUNDZ");
                foreach (var sound in sounds) {
                    if (GUILayout.Button(sound.name)) {
                        sound.Play();
                    }
                }
            }

            if (debugTracks) {
                GUILayout.TextField("TRAX");
                foreach (var track in tracks) {
                    if (GUILayout.Button(track.name)) {
                        if (track.isPlaying) {

                            // TODO: REMOVE
                            Debug.Log("STAHP");
                            track.Stop();
                        } else {
                            CueTrack(track.name);
                        }
                    }
                }
            }
        }
    }
}

