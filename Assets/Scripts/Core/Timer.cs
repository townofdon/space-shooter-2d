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
        UnscaledDeltaTime,
        UnscaledFixedDeltaTime,
    }

    public class Timer
    {
        public Timer(TimerDirection direction = TimerDirection.Decrement, TimerStep step = TimerStep.DeltaTime, float initialDuration = 1f) {
            _direction = direction;
            _step = step;
            _value = 0f;
            SetDuration(initialDuration);
            End(); // set timer value to end cursor
        }

        float _value = 0f; // goes from 0 to 1
        float _duration = 1f;
        TimerDirection _direction = TimerDirection.Decrement;
        TimerStep _step = TimerStep.DeltaTime;
        bool _continuous = false;
        bool _turnedOn = true; // treat a timer with a duration of zero as turned off

        // action callbacks
        System.Action<float> _onEnd;

        public float value => _value;
        public float duration => _duration;
        public float timeLeft => active ? GetTimeLeft() : 0f;
        public bool active => _turnedOn && !GetIsAtEnd();
        public bool tZero => GetIsAtStart();
        public bool tEnd => GetIsAtEnd();
        public bool continuous {
            get { return _continuous; }
            set { _continuous = value; }
        }

        public void SetOnEnd(System.Action<float> OnEnd) {
            _onEnd = OnEnd;
        }

        public void TurnOn() {
            _turnedOn = true;
        }

        public void TurnOff() {
            _turnedOn = false;
        }

        public void SetDuration(float value) {
            _duration = value;
            if (value <= 0f) {
                _turnedOn = false;
                End();
            } else {
                _turnedOn = true;
            }
            if (_duration <= 0f) _duration = 1f;
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
            if (GetIsAtEnd()) {
                if (_continuous) Start();
                return;
            }
            _value = _direction == TimerDirection.Increment
                ? Mathf.Clamp(_value + GetStepValue(), 0f, 1f)
                : Mathf.Clamp(_value - GetStepValue(), 0f, 1f);
            if (GetIsAtEnd() && _onEnd != null) _onEnd(_value);
            if (float.IsNaN(_value)) End();
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
            if (float.IsNaN(_value)) Start();
        }

        public IEnumerator WaitUntilFinished(bool tickInsideCoroutine = false) {
            if (_turnedOn) {
                while (active) {
                    if (tickInsideCoroutine) Tick();
                    yield return null;
                }
            }
        }

        public IEnumerator StartAndWaitUntilFinished(bool tickInsideCoroutine = false) {
            if (_turnedOn) {
                Start();
                yield return WaitUntilFinished(tickInsideCoroutine);
            }
        }

        // PRIVATE

        float GetStepValue() {
            if (_duration <= 0f) return 0f;
            if (Time.timeScale <= 0f && (_step == TimerStep.DeltaTime || _step == TimerStep.FixedDeltaTime)) return 0f;
            if (_step == TimerStep.DeltaTime && float.IsNaN(Time.deltaTime)) return 0f;
            if (_step == TimerStep.FixedDeltaTime && float.IsNaN(Time.fixedDeltaTime)) return 0f;
            switch (_step) {
                case TimerStep.DeltaTime:
                    return Time.deltaTime / _duration;
                case TimerStep.FixedDeltaTime:
                    return Time.fixedDeltaTime / _duration;
                case TimerStep.UnscaledDeltaTime:
                    return Time.unscaledDeltaTime / _duration;
                case TimerStep.UnscaledFixedDeltaTime:
                    return Time.fixedUnscaledDeltaTime / _duration;
                default:
                    return 0f;
            }
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

        float GetTimeLeft() {
            return _direction == TimerDirection.Increment
                ? _duration * (1f - value)
                : _duration * _value;
        }

        public override string ToString()
        {
            return "t=" + _value + " dur=" + _duration + " on=" + _turnedOn + " active=" + active;
        }
    }
}
