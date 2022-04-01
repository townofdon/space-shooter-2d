using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

using Core;

namespace Audio
{
    public enum PlayCursor {
        Stopped,
        Head,
        Loop,
        Tail,
    }

    [System.Serializable]
    public class LoopableSound : BaseSound
    {
        [SerializeField][Range(0f, 0.2f)] protected float pitchVariance = 0f;

        [SerializeField] AudioClip clipHead;
        [SerializeField] AudioClip clipLoop;
        [SerializeField] AudioClip clipTail;

        [SerializeField] bool playLoopToEnd = false;

        AudioSource sourceHead;
        AudioSource sourceLoop;
        AudioSource sourceTail;

        // cached
        double dspStartDelay = 0.01;
        double clipHeadDuration = 0.0;
        double clipLoopDuration = 0.0;
        double clipTailDuration = 0.0;
        double timeLoopStartScheduled = 0.0;
        double timeLoopEndScheduled = 0.0;
        MonoBehaviour _script;
        Coroutine playCoroutine;

        // state
        PlayCursor cursor = PlayCursor.Head;
        bool playButtonPressed = false;

        public override bool isPlaying => playButtonPressed
            || (cursor != PlayCursor.Stopped)
            || (sourceLoop != null && sourceLoop.isPlaying)
            || (sourceTail != null && sourceTail.isPlaying);
        public override bool hasClip => clipLoop != null;
        public override bool hasSource => sourceLoop != null;
        public PlayCursor Cursor => cursor;

        public override void Init(MonoBehaviour script, AudioMixerGroup mix = null)
        {
            _script = script;
            if (mix != null) mixerGroup = mix;

            if (clipHead != null) {
                sourceHead = script.gameObject.AddComponent<AudioSource>();
                SetSource(clipHead, sourceHead, false);
                clipHeadDuration = GetClipDuration(clipHead);
            }
            if (clipLoop != null) {
                sourceLoop = script.gameObject.AddComponent<AudioSource>();
                SetSource(clipLoop, sourceLoop, true);
                clipLoopDuration = GetClipDuration(clipLoop);
            }
            if (clipTail != null) {
                sourceTail = script.gameObject.AddComponent<AudioSource>();
                SetSource(clipTail, sourceTail, false);
                clipTailDuration = GetClipDuration(clipTail);
            }

            script.StartCoroutine(RealtimeEditorInspection());
        }

        void SetSource(AudioClip clip, AudioSource source, bool loop) {
            if (source == null) return;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.clip = clip;
            source.playOnAwake = false;
            source.ignoreListenerPause = ignoreListenerPause;
            source.outputAudioMixerGroup = mixerGroup;
            // 3d settings
            source.spread = spread;
            source.spatialBlend = spatialBlend;
            source.minDistance = minFalloffDistance;
            source.maxDistance = maxFalloffDistance;
        }

        public override void Play() {
            if (!ValidateSound()) return;
            if (playButtonPressed) return;

            if (sourceHead != null && sourceHead.isPlaying) sourceHead.Stop();
            if (sourceLoop != null && sourceLoop.isPlaying) sourceLoop.Stop();
            if (sourceTail != null && sourceTail.isPlaying) sourceTail.Stop();

            if (playCoroutine != null) _script.StopCoroutine(playCoroutine);
            if (sourceLoop != null) sourceLoop.pitch = Utils.RandomVariance(pitch, pitchVariance, 0f, 1f);
            playCoroutine = _script.StartCoroutine(IPlay());
        }

        IEnumerator IPlay() {
            playButtonPressed = true;

            if (sourceHead != null && sourceHead.enabled) {
                cursor = PlayCursor.Head;
                timeLoopStartScheduled = AudioSettings.dspTime + dspStartDelay + clipHeadDuration;
                sourceHead.PlayScheduled(AudioSettings.dspTime + dspStartDelay);
                sourceLoop.PlayScheduled(timeLoopStartScheduled);

                while (playButtonPressed && sourceHead.isPlaying) yield return null;

                sourceHead.Stop();
                if (playButtonPressed) cursor = PlayCursor.Loop;
            } else {
                cursor = PlayCursor.Loop;
                timeLoopStartScheduled = AudioSettings.dspTime + dspStartDelay;
                sourceLoop.PlayScheduled(timeLoopStartScheduled);
            }

            while (playButtonPressed) yield return null;

            if (cursor == PlayCursor.Loop && playLoopToEnd) {
                int numFullCycles = GetNumFullLoopCycles(sourceLoop.pitch);
                timeLoopEndScheduled = timeLoopStartScheduled + clipLoopDuration * numFullCycles * (double)sourceLoop.pitch;
            } else {
                // do not wait until loop end; start playing tail immediately
                timeLoopEndScheduled = AudioSettings.dspTime + dspStartDelay;
            }

            sourceLoop.SetScheduledEndTime(timeLoopEndScheduled);
            if (sourceTail != null && sourceTail.enabled) sourceTail.PlayScheduled(timeLoopEndScheduled);

            while (sourceLoop.isPlaying) yield return null;

            cursor = PlayCursor.Tail;
            
            while (sourceTail != null && sourceTail.isPlaying) yield return null;

            cursor = PlayCursor.Stopped;
            playCoroutine = null;
        }

        public override void Stop() {
            playButtonPressed = false;
        }

        protected override IEnumerator RealtimeEditorInspection() {
            while (true) {
                yield return new WaitForSecondsRealtime(1f);
                if (!realtimeEditorInspect) continue;
                
                if (clipHead != null) {
                    SetSource(clipHead, sourceHead, false);
                    clipHeadDuration = GetClipDuration(clipHead);
                }
                if (clipLoop != null) {
                    SetSource(clipLoop, sourceLoop, true);
                    clipLoopDuration = GetClipDuration(clipLoop);
                }
                if (clipTail != null) {
                    SetSource(clipTail, sourceTail, false);
                    clipTailDuration = GetClipDuration(clipTail);
                }
            }
        }

        bool ValidateSound() {
            if (sourceLoop == null) {
                return false;
            }
            return true;
        }

        double GetClipDuration(AudioClip clip) {
            return (double)clip.samples / clip.frequency;
        }

        // calculate timeLoopEnd based on amount of time has elapsed since timeLoopStart, vs. per the clipLoopDuration
        // e.g. what the number of cycles would be if the loop had finished
        int GetNumFullLoopCycles(double currentPitch = 1.0) {
            return Mathf.Min(1, Mathf.CeilToInt((float)((AudioSettings.dspTime - timeLoopStartScheduled) / (clipLoopDuration * currentPitch))));
        }
    }
}

