using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

using Core;

namespace Audio {
    [System.Serializable]
    public class Track : BaseSound {
        [SerializeField][Range(10f, 300f)] float bpm = 150f;
        [SerializeField][Range(1, 8)] int beatsPerBar = 16;
        [SerializeField] bool loops = true;

        [SerializeField] AudioClip clipIntro;
        [SerializeField] AudioClip clipLoop;
        [SerializeField] AudioClip clipOutro;

        [SerializeField][Range(0f, 5f)] float fadeInTime = 0f;
        [SerializeField][Range(0f, 5f)] float fadeOutTime = 0f;

        AudioSource sourceIntro;
        AudioSource sourceLoop;
        AudioSource sourceOutro;

        // cached
        double dspStartDelay = 0.01;
        double clipIntroDuration = 0.0;
        double clipLoopDuration = 0.0;
        double clipOutroDuration = 0.0;
        double timeFadeStarted = 0.0;
        double timeStart = 0.0;
        double timeLeft = 0.0;
        double timeElapsed = 0.0;
        double timeStopScheduled = 0.0;
        double timeLoopStartScheduled = 0.0;
        double timeLoopEndScheduled = 0.0;
        double timeOneBar = 0.0;
        MonoBehaviour _script;
        Coroutine playCoroutine;
        Coroutine fadeInCoroutine;
        Coroutine fadeOutCoroutine;
        Coroutine cuedStopCoroutine;

        // state
        PlayCursor cursor = PlayCursor.Stopped;
        bool playButtonPressed = false;
        float fadeVolume = 0f;
        float fadePct = 0f;

        public override bool isPlaying => playButtonPressed
            || (sourceIntro != null && sourceIntro.isPlaying)
            || (sourceLoop != null && sourceLoop.isPlaying)
            || (sourceOutro != null && sourceOutro.isPlaying);
        public override bool hasClip => clipLoop != null;
        public override bool hasSource => sourceLoop != null;
        public PlayCursor Cursor => cursor;

        public override void Init(MonoBehaviour script, AudioMixerGroup mix = null, AudioSource _source = null) {
            _script = script;
            if (mix != null) mixerGroup = mix;

            if (clipIntro != null) {
                sourceIntro = script.gameObject.AddComponent<AudioSource>();
                SetSource(clipIntro, sourceIntro, false);
                clipIntroDuration = GetClipDuration(clipIntro);
            }
            if (clipLoop != null) {
                sourceLoop = script.gameObject.AddComponent<AudioSource>();
                SetSource(clipLoop, sourceLoop, loops);
                clipLoopDuration = GetClipDuration(clipLoop);
            }
            if (clipOutro != null) {
                sourceOutro = script.gameObject.AddComponent<AudioSource>();
                SetSource(clipOutro, sourceOutro, false);
                clipOutroDuration = GetClipDuration(clipOutro);
            }

            if (bpm == 0f) bpm = 150f;

            script.StartCoroutine(RealtimeEditorInspection());
        }

        public void CueStart(double timeCueWait) {
            if (!ValidateSound()) return;
            if (playButtonPressed) return;

            ImperativelyStop();
            SetVolume(volume);
            playCoroutine = _script.StartCoroutine(IPlay(timeCueWait));
        }

        public double CueStop() {
            if (!ValidateSound()) return 0.0;
            if (!isPlaying) return 0.0;
            if (AudioSettings.dspTime < timeStopScheduled) return timeStopScheduled;

            // determine how much time is contained in one bar of music - per bpm
            timeOneBar = (60.0 * beatsPerBar) / (double)bpm;

            // determine how much time is left in currently playing clip
            timeLeft = 0.0;
            timeElapsed = 0.0;
            if (cursor == PlayCursor.Head) {
                timeLeft = GetClipTimeLeft(sourceIntro, clipIntro);
                timeElapsed = GetClipTimeElapsed(sourceIntro, clipIntro);
            } else if (cursor == PlayCursor.Loop) {
                timeLeft = GetClipTimeLeft(sourceLoop, clipLoop);
                timeElapsed = GetClipTimeElapsed(sourceLoop, clipLoop);
                // int numLoopsCompleted = GetNumCompletedLoopCycles();
                // timeLeftInClip = AudioSettings.dspTime + dspStartDelay - (timeClipStarted + clipLoopDuration * numLoopsCompleted);
            } else if (cursor == PlayCursor.Tail) {
                timeLeft = GetClipTimeLeft(sourceOutro, clipOutro);
                timeElapsed = GetClipTimeElapsed(sourceOutro, clipOutro);
            }

            if (timeLeft < timeOneBar) {
                // assume that the end of each clip lines up perfectly to the beat
                timeStopScheduled = AudioSettings.dspTime + timeLeft;
            } else {
                // get time to end of the bar
                timeStopScheduled = AudioSettings.dspTime + timeOneBar - (timeElapsed % timeOneBar);
            }

            if (sourceIntro != null) sourceLoop.SetScheduledEndTime(timeStopScheduled);
            if (sourceLoop != null) sourceLoop.SetScheduledEndTime(timeStopScheduled);
            if (sourceOutro != null) sourceLoop.SetScheduledEndTime(timeStopScheduled);

            if (cuedStopCoroutine != null) _script.StopCoroutine(cuedStopCoroutine);
            cuedStopCoroutine = _script.StartCoroutine(ICuedStop());

            return timeStopScheduled;
        }

        public override void Play() {
            if (!ValidateSound()) return;
            if (playButtonPressed) return;

            ImperativelyStop();
            playCoroutine = _script.StartCoroutine(IPlay(0.0));
            fadeInCoroutine = _script.StartCoroutine(IFadeIn());
        }

        public override void Stop() {
            playButtonPressed = false;
            if (fadeInCoroutine != null) _script.StopCoroutine(fadeInCoroutine);
            if (fadeOutCoroutine != null) _script.StopCoroutine(fadeOutCoroutine);
            if (cuedStopCoroutine != null) _script.StopCoroutine(cuedStopCoroutine);
            fadeOutCoroutine = _script.StartCoroutine(IFadeOut());
        }

        IEnumerator IPlay(double timeCueWait = 0.0) {
            playButtonPressed = true;
            timeStart = timeCueWait > 0
                ? timeCueWait
                : AudioSettings.dspTime + dspStartDelay;

            if (sourceIntro != null && sourceIntro.enabled) {
                cursor = PlayCursor.Head;
                timeLoopStartScheduled = timeStart + clipIntroDuration;
                sourceIntro.PlayScheduled(timeStart);
                sourceLoop.PlayScheduled(timeLoopStartScheduled);

                while (playButtonPressed && sourceIntro.isPlaying) yield return null;

                if (playButtonPressed) {
                    cursor = PlayCursor.Loop;
                }
            } else {
                cursor = PlayCursor.Loop;
                timeLoopStartScheduled = timeStart;
                sourceLoop.PlayScheduled(timeLoopStartScheduled);
            }

            while (playButtonPressed && sourceLoop.isPlaying) yield return null;

            int numFullCycles = GetNumFullLoopCycles();
            timeLoopEndScheduled = timeLoopStartScheduled + clipLoopDuration * numFullCycles;

            if (sourceLoop.isPlaying) sourceLoop.SetScheduledEndTime(timeLoopEndScheduled);
            if (sourceOutro != null && sourceOutro.enabled) sourceOutro.PlayScheduled(timeLoopEndScheduled);

            while (sourceLoop.isPlaying) yield return null;

            cursor = PlayCursor.Tail;

            while (sourceOutro != null && sourceOutro.isPlaying) yield return null;

            cursor = PlayCursor.Stopped;
            playButtonPressed = false;
            playCoroutine = null;
        }

        IEnumerator ICuedStop() {
            while (sourceIntro != null && sourceIntro.isPlaying) yield return null;
            while (sourceLoop != null && sourceLoop.isPlaying) yield return null;
            while (sourceOutro != null && sourceOutro.isPlaying) yield return null;
            ImperativelyStop();
        }

        IEnumerator IFadeIn() {
            if (playButtonPressed && fadeInTime > 0f) {
                timeFadeStarted = AudioSettings.dspTime + dspStartDelay;
                fadeVolume = 0f;

                while (playButtonPressed && (AudioSettings.dspTime - timeFadeStarted) < fadeInTime) {
                    fadePct = (float)(AudioSettings.dspTime - timeFadeStarted) / fadeInTime;
                    fadeVolume = Mathf.Lerp(0f, volume, Easing.easeInOutQuart(fadePct));
                    SetVolume(fadeVolume);
                    yield return null;
                }

                SetVolume(volume);
                fadeVolume = 0f;
            }

            fadeInCoroutine = null;
        }

        IEnumerator IFadeOut() {
            if (isPlaying && fadeOutTime > 0f) {
                timeFadeStarted = AudioSettings.dspTime;
                // if fadeVolume is > 0, that implies that the track was stopped while fading in; we want to start the fadeOut from THAT point rather than a jarring change to full volume
                fadeVolume = fadeVolume > 0 ? fadeVolume : volume;

                while (isPlaying && (AudioSettings.dspTime - timeFadeStarted) < fadeOutTime) {
                    fadePct = (float)(AudioSettings.dspTime - timeFadeStarted) / fadeOutTime;
                    fadeVolume = Mathf.Lerp(volume, 0f, Easing.easeInOutQuart(fadePct));
                    SetVolume(fadeVolume);
                    yield return null;
                }

                SetVolume(0f);
            }

            ImperativelyStop();
            fadeOutCoroutine = null;
        }

        protected override IEnumerator RealtimeEditorInspection() {
            while (true) {
                yield return new WaitForSecondsRealtime(1f);
                if (!realtimeEditorInspect) continue;

                if (clipIntro != null) {
                    SetSource(clipIntro, sourceIntro, false);
                    clipIntroDuration = GetClipDuration(clipIntro);
                }
                if (clipLoop != null) {
                    SetSource(clipLoop, sourceLoop, true);
                    clipLoopDuration = GetClipDuration(clipLoop);
                }
                if (clipOutro != null) {
                    SetSource(clipOutro, sourceOutro, false);
                    clipOutroDuration = GetClipDuration(clipOutro);
                }
            }
        }

        void SetSource(AudioClip clip, AudioSource source, bool loop) {
            if (source == null) return;
            source.volume = volume;
            source.pitch = 1f;
            source.loop = loop;
            source.clip = clip;
            source.playOnAwake = false;
            source.ignoreListenerPause = true;
            source.outputAudioMixerGroup = mixerGroup;
            // 3d settings
            source.spread = spread;
            source.spatialBlend = spatialBlend;
            source.minDistance = minFalloffDistance;
            source.maxDistance = maxFalloffDistance;
        }

        void SetVolume(float value) {
            if (sourceIntro != null) sourceIntro.volume = value;
            if (sourceLoop != null) sourceLoop.volume = value;
            if (sourceOutro != null) sourceOutro.volume = value;
        }

        void ImperativelyStop() {
            playButtonPressed = false;
            if (playCoroutine != null) _script.StopCoroutine(playCoroutine);
            if (fadeInCoroutine != null) _script.StopCoroutine(fadeInCoroutine);
            if (fadeOutCoroutine != null) _script.StopCoroutine(fadeOutCoroutine);
            if (cuedStopCoroutine != null) _script.StopCoroutine(cuedStopCoroutine);
            if (sourceIntro != null) sourceIntro.Stop();
            if (sourceLoop != null) sourceLoop.Stop();
            if (sourceOutro != null) sourceOutro.Stop();
            cursor = PlayCursor.Stopped;
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

        double GetClipTimeElapsed(AudioSource source, AudioClip clip) {
            if (source == null || clip == null) return 0.0;
            return (double)source.timeSamples / clip.frequency;
        }

        double GetClipTimeLeft(AudioSource source, AudioClip clip) {
            if (source == null || clip == null) return 0.0;
            return GetClipDuration(clip) - GetClipTimeElapsed(source, clip);
        }

        // calculate timeLoopEnd based on amount of time has elapsed since timeLoopStart, vs. per the clipLoopDuration
        // e.g. what the number of cycles would be if the loop had finished
        int GetNumFullLoopCycles() {
            return Mathf.Min(1, Mathf.CeilToInt((float)((AudioSettings.dspTime - timeLoopStartScheduled) / (clipLoopDuration))));
        }

        int GetNumCompletedLoopCycles() {
            return Mathf.Min(1, Mathf.FloorToInt((float)((AudioSettings.dspTime - timeLoopStartScheduled) / (clipLoopDuration))));
        }
    }
}

