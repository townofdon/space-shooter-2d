using System.Collections;
using UnityEngine;

namespace Core
{

    public enum TimerDirection {
        Increment,
        Decrement,
    }

    public enum TimerStep {
        DeltaTime,
        FixedDeltaTime,
    }

    public class Timer
    {
        public Timer(TimerDirection direction = TimerDirection.Decrement, TimerStep step = TimerStep.DeltaTime) {
            _direction = direction;
            _step = step;
            _value = 0f;
            End(); // set timer value to end cursor
        }

        float _value;
        float _duration = 1f;
        TimerDirection _direction = TimerDirection.Decrement;
        TimerStep _step = TimerStep.DeltaTime;
        bool _continuous = false;
        bool _turnedOn = true; // treat a timer with a duration of zero as turned off

        public float value => _value;
        public bool active => _turnedOn && !GetIsAtEnd();
        public bool tZero => GetIsAtStart();
        public bool tEnd => GetIsAtEnd();
        public bool continuous {
            get { return _continuous; }
            set{ _continuous = value; }
        }

        public void SetDuration(float duration, float value = 0f) {
            if (_duration <= 0f) {
                _turnedOn = false;
                End();
            }
            _duration = duration;
            _value = Mathf.Clamp01(value);
        }

        public void SetValue(float value) {
            if (!_turnedOn) return;
            _value = Mathf.Clamp01(value);
        }

        public void Start() {
            _value = _direction == TimerDirection.Increment
                ? 0f
                : 1f;
        }

        public void End() {
            _value = _direction == TimerDirection.Increment
                ? 1f
                : 0f;
        }

        public void Tick() {
            if (!_turnedOn) return;
            if (_continuous && GetIsAtEnd()) {
                Start();
                return;
            }
            _value = _direction == TimerDirection.Increment
                ? Mathf.Clamp(_value + GetStepValue(), 0f, 1f)
                : Mathf.Clamp(_value - GetStepValue(), 0f, 1f);
        }

        public void TickReversed() {
            if (!_turnedOn) return;
            if (_continuous && GetIsAtStart()) {
                End();
                return;
            }
            _value = _direction == TimerDirection.Increment
                ? Mathf.Clamp(_value - GetStepValue(), 0f, 1f)
                : Mathf.Clamp(_value + GetStepValue(), 0f, 1f);
        }

        public IEnumerator WaitUntilFinished(bool tickInsideCoroutine = false) {
            while(active) {
                if (tickInsideCoroutine) Tick();
                yield return null;
            }
        }

        public IEnumerator StartAndWaitUntilFinished(bool tickInsideCoroutine = false) {
            Start();
            yield return WaitUntilFinished();
        }

        // PRIVATE

        float GetStepValue() {
            return _step == TimerStep.DeltaTime
                ? Time.deltaTime / _duration
                : Time.fixedDeltaTime / _duration;
        }

        bool GetIsAtStart() {
            return _direction == TimerDirection.Increment
                ? _value == 0f
                : _value == 1f;
        }

        bool GetIsAtEnd() {
            return _direction == TimerDirection.Increment
                ? _value == 1f
                : _value == 0f;
        }

        public override string ToString()
        {
            return "t=" + _value + " active=" + active;
        }
    }
}
