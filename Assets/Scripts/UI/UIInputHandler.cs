using UnityEngine;
using UnityEngine.InputSystem;

using Core;
using Event;

namespace UI {

    public class UIInputHandler : MonoBehaviour {
        [SerializeField] EventChannelSO eventChannel;

        // components
        PlayerInput input;

        // cached
        Vector2 _move;

        // state
        bool _isPaused = false;
        bool _isDebug = false;
        bool _isSubmitting = false;
        bool _isCanceling = false;

        // public
        public Vector2 move => _move;
        public bool isSubmitting => _isSubmitting;
        public bool isCanceling => _isCanceling;

        void OnEnable() {
            eventChannel.OnPause.Subscribe(OnPause);
            eventChannel.OnUnpause.Subscribe(OnUnpause);
        }

        void OnDisable() {
            eventChannel.OnPause.Unsubscribe(OnPause);
            eventChannel.OnUnpause.Unsubscribe(OnUnpause);
        }

        void Start() {
            AppIntegrity.AssertPresent(eventChannel);
            input = GetComponent<PlayerInput>();
            input.SwitchCurrentActionMap("UI");
            input.enabled = true;
        }

        void OnNavigate(InputValue value) {
            if (_isPaused) return;
            _move = value.Get<Vector2>();
        }

        void OnStart(InputValue value) {
            if (!value.isPressed) return;
            if (_isPaused) {
                eventChannel.OnUnpause.Invoke();
            } else {
                eventChannel.OnPause.Invoke();
            }
        }

        void OnUIStart(InputValue value) {
            if (!value.isPressed) return;
            OnStart(value);
        }

        void OnAnyKey(InputValue value) {
            if (!value.isPressed) return;
            eventChannel.OnAnyKeyPress.Invoke();
        }

        void OnUIAnyKey(InputValue value) {
            if (!value.isPressed) return;
            OnAnyKey(value);
        }

        void OnSubmit(InputValue value) {
            _isSubmitting = value.isPressed;
        }

        void OnCancel(InputValue value) {
            _isCanceling = value.isPressed;
        }


        // event callbacks

        void OnPause() {
            input.SwitchCurrentActionMap("UI");
            _isPaused = true;
        }

        void OnUnpause() {
            input.SwitchCurrentActionMap("UI");
            _isPaused = false;
        }
    }
}
