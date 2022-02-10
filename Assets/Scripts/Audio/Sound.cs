using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

using Core;

namespace Audio
{

    [System.Serializable]
    public class Sound : BaseSound
    {
        [SerializeField] string soundName;
        [SerializeField] AudioClip[] clips;
        [SerializeField] bool oneShot = true;

        [HideInInspector]
        AudioSource source;

        // getters
        public string name => soundName;
        public AudioClip Clip => clips[currentClipIndex];
        public override bool isPlaying => source != null && source.isPlaying;

        // state
        float volumeFadeStart = 0f;
        Timer fadeTimer = new Timer();
        int currentClipIndex = 0;

        public override void Init(MonoBehaviour script, AudioMixerGroup mix = null)
        {
            // nullSound.SetSource(gameObject.AddComponent<AudioSource>(), soundFXMix);
            source = script.gameObject.AddComponent<AudioSource>();
            source.volume = volume;
            source.pitch = pitch;
            source.loop = false;
            source.clip = clips[0];
            source.playOnAwake = false;
            source.outputAudioMixerGroup = mix;
            source.ignoreListenerPause = ignoreListenerPause;
            AppIntegrity.AssertPresent<AudioClip>(clips[0]);

            script.StartCoroutine(RealtimeEditorInspection());
        }

        public override void Play()
        {
            ValidateSound();
            if (oneShot) {
                UpdateVariance();
                source.PlayOneShot(clips[currentClipIndex]);
            } else {
                if (source.isPlaying) return;
                UpdateVariance();
                source.Play();
            }
        }

        public void PlayAtLocation(Vector3 location)
        {
            ValidateSound();
            UpdateVariance();
            AudioSource.PlayClipAtPoint(clips[currentClipIndex], location, volume);
        }

        public override void Stop()
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

        protected override IEnumerator RealtimeEditorInspection() {
            while (true) {
                yield return new WaitForSecondsRealtime(1f);
                if (!realtimeEditorInspect) continue;
                if (source == null) continue;

                source.volume = volume;
                source.pitch = pitch;
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
    }
}

