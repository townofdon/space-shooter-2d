using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Audio;

using Core;

namespace Audio
{

    [Serializable]
    public class Sound
    {
        [SerializeField] string soundName;
        [SerializeField] AudioClip[] clips;

        [SerializeField][Range(0f, 1f)] float volume = 0.7f;
        [SerializeField][Range(.1f, 3f)] float pitch = 1f;
        [SerializeField] bool loop = false;
        [SerializeField] bool oneShot = false;
        [SerializeField][Range(0f, 0.5f)] float volumeVariance = 0.1f;
        [SerializeField][Range(0f, 0.5f)] float pitchVariance = 0.1f;

        [HideInInspector]
        AudioSource source;
        
        // getters
        public string name => soundName;
        public AudioClip Clip => clips[currentClipIndex];
        public bool isPlaying => source != null && source.isPlaying;

        // state
        float volumeFadeStart = 0f;
        Timer fadeTimer = new Timer();
        int currentClipIndex = 0;

        public void Init(GameObject gameObject, AudioMixerGroup mix = null)
        {
            // nullSound.SetSource(gameObject.AddComponent<AudioSource>(), soundFXMix);
            source = gameObject.AddComponent<AudioSource>();
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.clip = clips[0];
            source.playOnAwake = false;
            source.outputAudioMixerGroup = mix;
            AppIntegrity.AssertNonEmptyString(soundName);
            AppIntegrity.AssertPresent<AudioClip>(clips[0]);
        }

        public void Play()
        {
            ValidateSound();
            if (loop || !oneShot)
            {
                if (source.isPlaying) return;
                UpdateVariance();
                source.Play();
            } else {
                UpdateVariance();
                source.PlayOneShot(clips[currentClipIndex]);
            }
        }

        public void PlayAtLocation(Vector3 location)
        {
            ValidateSound();
            UpdateVariance();
            AudioSource.PlayClipAtPoint(clips[currentClipIndex], location, volume);
        }

        public void Stop()
        {
            ValidateSound();
            source.Stop();
        }

        // PRIVATE

        void UpdateVariance()
        {
            currentClipIndex = UnityEngine.Random.Range(0, clips.Length);
            source.clip = clips[currentClipIndex];
            source.volume = Utils.RandomVariance(volume, volumeVariance, 0f, 1f);
            source.pitch = Utils.RandomVariance(pitch, pitchVariance, 0f, 1f);
        }

        void ValidateSound() {
            if (source == null) {
                throw new UnityException("Audio.Sound: \"" + soundName + "\" has no source");
            }
        }

        // MUSIC TRACK METHODS

        // public IEnumerator FadeIn(float duration)
        // {
        //     volumeFadeStart = source.volume;
        //     timeFade = 0f;
        //     yield return null;

        //     while (source.volume < volume || timeFade < duration)
        //     {
        //         // TODO: ADD EASING - currently is only a linear fadeout
        //         source.volume = Mathf.Lerp(volumeFadeStart, volume, timeFade / duration);
        //         timeFade += Time.deltaTime;
        //         yield return null;
        //     }

        //     yield return null;
        // }

        // public IEnumerator FadeOut(float duration)
        // {
        //     volumeFadeStart = source.volume;
        //     timeFade = 0f;
        //     yield return null;

        //     while (source.volume > 0f || timeFade < duration)
        //     {
        //         // TODO: ADD EASING - currently is only a linear fadeout
        //         source.volume = Mathf.Lerp(volumeFadeStart, 0f, timeFade / duration);
        //         timeFade += Time.deltaTime;
        //         yield return null;
        //     }

        //     yield return null;
        // }

        // public void PlayTrack()
        // {
        //     if (source.isPlaying) return;

        //     source.UnPause();
        //     source.Play();
        // }

        // public void PauseTrack()
        // {
        //     source.Pause();
        // }

        // public void UnPauseTrack()
        // {
        //     source.UnPause();
        // }

        // public void SilenceTrack()
        // {
        //     source.volume = 0f;
        // }

        // public void UnsilenceTrack()
        // {
        //     source.volume = volume;
        // }

        public IEnumerator RealtimeEditorInspection() {
            while (true) {
                yield return new WaitForSecondsRealtime(1f);
                if (source == null) continue;

                source.volume = volume;
                source.pitch = pitch;
                source.loop = loop;
            }
        }
    }
}

