using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

using Core;

namespace Audio
{

    [System.Serializable]
    public class Sound : BaseSound
    {
        [SerializeField][Range(0f, 0.5f)] protected float volumeVariance = 0.1f;
        [SerializeField][Range(0f, 0.5f)] protected float pitchVariance = 0.1f;
        [SerializeField][Range(0f, 0.03f)] protected float delayVariance = 0f;
        [SerializeField] AudioClip[] clips;
        [SerializeField] bool oneShot = true;

        [HideInInspector]
        AudioSource source;

        // getters
        public AudioClip Clip => clips[currentClipIndex];
        public override bool isPlaying => source != null && source.isPlaying;
        public override bool hasClip => clips.Length > 0;
        public override bool hasSource => source != null;

        // state
        float volumeFadeStart = 0f;
        Timer fadeTimer = new Timer();
        int currentClipIndex = 0;

        public override void Init(MonoBehaviour script, AudioMixerGroup mix = null)
        {
            if (clips.Length == 0) return;
            // nullSound.SetSource(gameObject.AddComponent<AudioSource>(), soundFXMix);
            source = script.gameObject.AddComponent<AudioSource>();
            source.volume = volume;
            source.pitch = pitch;
            source.loop = false;
            source.clip = clips[0];
            source.playOnAwake = false;
            source.outputAudioMixerGroup = mix;
            source.ignoreListenerPause = ignoreListenerPause;

            // 3d settings
            source.dopplerLevel = dopplerLevel;
            source.spread = spread;
            source.spatialBlend = spatialBlend;
            source.minDistance = minFalloffDistance;
            source.maxDistance = maxFalloffDistance;
            AppIntegrity.AssertPresent<AudioClip>(clips[0]);

            script.StartCoroutine(RealtimeEditorInspection());
        }

        public override void Play()
        {
            if (!ValidateSound()) return;
            if (oneShot) {
                UpdateVariance();
                source.PlayOneShot(clips[currentClipIndex]);
            } else {
                UpdateVariance();
                source.Stop();
                source.PlayDelayed(UnityEngine.Random.Range(0f, delayVariance));
            }
        }

        public void PlayAtLocation(Vector3 location)
        {
            if (!ValidateSound()) return;
            UpdateVariance();
            AudioSource.PlayClipAtPoint(clips[currentClipIndex], location, volume);
        }

        public override void Stop()
        {
            if (!ValidateSound()) return;
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

        bool ValidateSound() {
            if (clips.Length == 0 || source == null) {
                return false;
            }
            return true;
        }

        protected override IEnumerator RealtimeEditorInspection() {
            while (true) {
                yield return new WaitForSecondsRealtime(1f);
                if (!realtimeEditorInspect) continue;
                if (source == null) continue;

                source.volume = volume;
                source.pitch = pitch;

                // 3d settings
                source.spread = spread;
                source.spatialBlend = spatialBlend;
                source.minDistance = minFalloffDistance;
                source.maxDistance = maxFalloffDistance;
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

