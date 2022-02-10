using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

using Core;

namespace Audio
{
    enum PlayCursor {
        Stopped,
        Head,
        Loop,
        Tail,
    }

    [System.Serializable]
    public class LoopableSound : BaseSound
    {
        [SerializeField] AudioClip clipHead;
        [SerializeField] AudioClip clipLoop;
        [SerializeField] AudioClip clipTail;

        AudioSource sourceHead;
        AudioSource sourceLoop;
        AudioSource sourceTail;

        // cached
        double dspStartDelay = 0.01;
        double clipHeadDuration = 0.0;
        double clipLoopDuration = 0.0;
        double clipTailDuration = 0.0;
        // TODO: REMOVE
        // double timeLoopStart = 0.0;
        // double timeLoopEnd = 0.0;
        MonoBehaviour _script;
        Coroutine playCoroutine;

        // state
        PlayCursor cursor = PlayCursor.Head;
        double timePlaying = 0.0;
        double nextStartTime = 0.0;
        bool startButtonPressed = false;

        public override bool isPlaying => startButtonPressed;

        public override void Init(MonoBehaviour script, AudioMixerGroup mix = null)
        {
            _script = script;
            if (mix != null) mixerGroup = mix;

            if (clipHead != null) {
                sourceHead = script.gameObject.AddComponent<AudioSource>();
                SetSource(clipHead, sourceHead, false);
                this.clipHeadDuration = GetClipDuration(clipHead);
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

            AppIntegrity.AssertPresent<AudioClip>(clipLoop);

            script.StartCoroutine(RealtimeEditorInspection());

            // This gets the exact duration of the first clip, note that you have to cast the samples value as a double
            double clipHeadDuration = (double)sourceHead.clip.samples / sourceHead.clip.frequency;
        }

        void SetSource(AudioClip clip, AudioSource source, bool loop) {
            source.volume = volume;
            source.pitch = pitch;
            source.loop = loop;
            source.clip = clip;
            source.playOnAwake = false;
            source.ignoreListenerPause = ignoreListenerPause;
            source.outputAudioMixerGroup = mixerGroup;
        }

        public override void Play() {
            // if head exists, start playing head --> time T0
                // queue up loop to start playing at time T1 (PlayScheduled)
            // else start playing loop --> T0 == T1

            // while (keep playing loop)
                // do nothing
                // if Stop signal is received, enqueue tail if exists to start playing at time T2
                // prepare loop to stop playing at time T2

            // let tail play through
            // If Play() signal received:
                // stop Tail immediately
                // start from beginning state

            if (startButtonPressed) return;

            if (sourceHead.isPlaying) sourceHead.Stop();
            if (sourceLoop.isPlaying) sourceLoop.Stop();
            if (sourceTail.isPlaying) sourceTail.Stop();

            if (playCoroutine != null) _script.StopCoroutine(playCoroutine);
            playCoroutine = _script.StartCoroutine(IPlay());
        }

        IEnumerator IPlay() {
            startButtonPressed = true;

            if (sourceHead != null) {
                cursor = PlayCursor.Head;
                sourceHead.PlayScheduled(AudioSettings.dspTime + dspStartDelay);
                sourceLoop.PlayScheduled(AudioSettings.dspTime + dspStartDelay + clipHeadDuration);

                while (sourceHead.isPlaying) yield return null;

                cursor = PlayCursor.Loop;
            } else {
                cursor = PlayCursor.Loop;
                sourceLoop.PlayScheduled(AudioSettings.dspTime + dspStartDelay);
            }

            while (startButtonPressed) yield return null;

            // do not wait until loop end; start playing tail immediately
            sourceLoop.SetScheduledEndTime(AudioSettings.dspTime + dspStartDelay);

            if (sourceTail != null) {
                cursor = PlayCursor.Tail;
                sourceTail.PlayScheduled(AudioSettings.dspTime + dspStartDelay);

                while (sourceTail.isPlaying) yield return null;
            }

            cursor = PlayCursor.Stopped;
            playCoroutine = null;
        }

        public override void Stop() {
            startButtonPressed = false;
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

        double GetClipDuration(AudioClip clip) {
            return (double)clip.samples / clip.frequency;
        }
    }
}

