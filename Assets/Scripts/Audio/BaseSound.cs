using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{

    [Serializable]
    public abstract class BaseSound
    {
        [SerializeField][Range(0f, 1f)] protected float volume = 0.7f;
        [SerializeField][Range(.1f, 3f)] protected float pitch = 1f;
        [SerializeField][Range(0f, 0.5f)] protected float volumeVariance = 0.1f;
        [SerializeField][Range(0f, 0.5f)] protected float pitchVariance = 0.1f;
        [SerializeField] protected bool ignoreListenerPause = false; // see - https://gamedevbeginner.com/10-unity-audio-tips-that-you-wont-find-in-the-tutorials/#audiolistener_pause
        [SerializeField] protected bool realtimeEditorInspect = false;
        [SerializeField] protected AudioMixerGroup mixerGroup = null;

        // getters
        public abstract bool isPlaying { get; }

        // public methods

        public abstract void Init(MonoBehaviour script, AudioMixerGroup mix = null);

        public abstract void Play();

        public abstract void Stop();

        // PRIVATE

        protected abstract IEnumerator RealtimeEditorInspection();

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

