using UnityEngine;
using UnityEngine.InputSystem;

using Core;
using Event;

namespace UI {

    public class KeyPress {
        public string text;
        public bool isPressed = false;
        public void AcknowledgePress() {
            isPressed = false;
        }
    }

    public class BackSpace {
        public bool isPressed = false;
        public void AcknowledgePress() {
            isPressed = false;
        }
    }

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
        KeyPress _keyPress = new KeyPress();
        BackSpace _backSpace = new BackSpace();

        // public
        public Vector2 move => _move;
        public bool isSubmitting => _isSubmitting;
        public bool isCanceling => _isCanceling;
        public KeyPress keyPress => _keyPress;
        public BackSpace backSpace => _backSpace;

        public void ResetInputs() {
            _isSubmitting = false;
            _isCanceling = false;
            _keyPress.AcknowledgePress();
            _backSpace.AcknowledgePress();

        }

        void OnEnable() {
            eventChannel.OnPause.Subscribe(OnPause);
            eventChannel.OnUnpause.Subscribe(OnUnpause);
            Keyboard.current.onTextInput += OnTextInput;
        }

        void OnDisable() {
            eventChannel.OnPause.Unsubscribe(OnPause);
            eventChannel.OnUnpause.Unsubscribe(OnUnpause);
            Keyboard.current.onTextInput -= OnTextInput;
        }

        void Start() {
            AppIntegrity.AssertPresent(eventChannel);
            input = GetComponent<PlayerInput>();
            // input.actions.FindActionMap("Player").Enable();
            // input.actions.FindActionMap("UI").Enable();
            input.SwitchCurrentActionMap("UI");
            input.enabled = true;
        }

        void OnNavigate(InputValue value) {
            if (_isPaused) return;
            _move = value.Get<Vector2>();
        }

        // void OnStart(InputValue value) {
        //     if (!value.isPressed) return;
        //     if (_isPaused) {
        //         eventChannel.OnUnpause.Invoke();
        //     } else {
        //         eventChannel.OnPause.Invoke();
        //     }
        // }

        // void OnUIStart(InputValue value) {
        //     if (!value.isPressed) return;
        //     if (_isPaused) {
        //         eventChannel.OnUnpause.Invoke();
        //     } else {
        //         eventChannel.OnPause.Invoke();
        //     }
        // }

        void OnAnyKey(InputValue value) {
            if (!value.isPressed) return;
            eventChannel.OnAnyKeyPress.Invoke();
        }

        void OnUIAnyKey(InputValue value) {
            if (!value.isPressed) return;
            eventChannel.OnAnyKeyPress.Invoke();
        }

        void OnSubmit(InputValue value) {
            _isSubmitting = value.isPressed;
        }

        void OnCancel(InputValue value) {
            _isCanceling = value.isPressed;
        }

        void OnTextInput(char ch) {
            keyPress.isPressed = true;
            keyPress.text = ch.ToString().ToUpper();
        }

        void OnBackspace(InputValue value) {
            _backSpace.isPressed = value.isPressed;
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
