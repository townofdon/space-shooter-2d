using UnityEngine;
using UnityEngine.InputSystem;

using Core;
using Event;
using Dialogue;

namespace Player {

    public enum PlayerControlMode {
        ByPlayer,
        ByGame,
    }

    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] EventChannelSO eventChannel;

        // components
        PlayerInput input;

        // cached
        Vector2 _move;
        Vector2 _look;
        bool _isFirePressed = false;
        bool _isFire2Pressed = false;
        bool _isMeleePressed = false;
        bool _isReloadPressed = false;
        bool _isBoostPressed = false;
        bool _isSwitchWeaponPressed = false;
        bool _isSwitchWeapon2Pressed = false;

        public PlayerControlMode controlMode => _controlMode;
        public Vector2 move => _move;
        public Vector2 look => _look;
        public bool isFirePressed => _isFirePressed;
        public bool isFire2Pressed => _isFire2Pressed;
        public bool isMeleePressed => _isMeleePressed;
        public bool isReloadPressed => _isReloadPressed;
        public bool isBoostPressed => _isBoostPressed;
        public bool isSwitchWeaponPressed => _isSwitchWeaponPressed;
        public bool isSwitchWeapon2Pressed => _isSwitchWeapon2Pressed;

        // cached
        Rigidbody2D rb;

        // state
        PlayerControlMode _controlMode;
        bool _isPaused = false;
        bool _isDebug = false;
        bool _isDisabled = false;
        Transform _autoMoveTarget;


        public void SetMode(PlayerControlMode value) {
            if (_controlMode == PlayerControlMode.ByGame && value == PlayerControlMode.ByPlayer) {
                rb.velocity = Vector2.zero;
            }
            _controlMode = value;
        }

        public void SetAutoMoveTarget(Transform value) {
            _autoMoveTarget = value;
        }

        Vector2 GetMoveTarget() {
            if (_autoMoveTarget == null) return Vector2.zero;
            return _autoMoveTarget.position;
        }

        void OnEnable() {
            eventChannel.OnPause.Subscribe(OnPause);
            eventChannel.OnUnpause.Subscribe(OnUnpause);
            eventChannel.OnShowDialogue.Subscribe(OnDisableInput);
            eventChannel.OnDismissDialogue.Subscribe(OnEnableInput);
        }

        void OnDisable() {
            eventChannel.OnPause.Unsubscribe(OnPause);
            eventChannel.OnUnpause.Unsubscribe(OnUnpause);
            eventChannel.OnShowDialogue.Unsubscribe(OnDisableInput);
            eventChannel.OnDismissDialogue.Unsubscribe(OnEnableInput);
        }

        void OnEnableInput() {
            _isDisabled = false;
        }
        void OnDisableInput() {
            _isDisabled = true;
        }
        void OnDisableInput(DialogueItemSO item) {
            OnDisableInput();
        }

        void Start() {
            AppIntegrity.AssertPresent(eventChannel);
            input = GetComponent<PlayerInput>();
            rb = GetComponent<Rigidbody2D>();
            input.SwitchCurrentActionMap("Player");
        }

        void Update() {
            if (_controlMode == PlayerControlMode.ByPlayer) return;
            _move = (GetMoveTarget() - (Vector2)transform.position).normalized;
            if (Vector2.Distance(GetMoveTarget(), transform.position) < 0.1f) SetMode(PlayerControlMode.ByPlayer);
        }

        void OnMove(InputValue value) {
            if (_controlMode == PlayerControlMode.ByGame) return;
            if (_isPaused || _isDisabled) return;
            _move = value.Get<Vector2>();
        }

        void OnLook(InputValue value) {
            if (_controlMode == PlayerControlMode.ByGame) return;
            if (_isPaused || _isDisabled) return;
            _look = value.Get<Vector2>();
        }

        void OnFire(InputValue value) {
            if (_controlMode == PlayerControlMode.ByGame) return;
            if (_isPaused || _isDisabled) return;
            _isFirePressed = value.isPressed;
        }

        void OnFireSecondary(InputValue value) {
            if (_controlMode == PlayerControlMode.ByGame) return;
            if (_isPaused || _isDisabled) return;
            _isFire2Pressed = value.isPressed;
        }

        void OnMelee(InputValue value) {
            if (_controlMode == PlayerControlMode.ByGame) return;
            if (_isPaused || _isDisabled) return;
            _isMeleePressed = value.isPressed;
        }

        void OnReload(InputValue value) {
            if (_controlMode == PlayerControlMode.ByGame) return;
            if (_isPaused || _isDisabled) return;
            _isReloadPressed = value.isPressed;
        }

        void OnBoost(InputValue value) {
            if (_controlMode == PlayerControlMode.ByGame) return;
            if (_isPaused || _isDisabled) return;
            _isBoostPressed = value.isPressed;
        }

        void OnSwitchWeapon(InputValue value) {
            if (_controlMode == PlayerControlMode.ByGame) return;
            if (_isPaused || _isDisabled) return;
            _isSwitchWeaponPressed = value.isPressed;
        }

        void OnSwitchSecondary(InputValue value) {
            if (_controlMode == PlayerControlMode.ByGame) return;
            if (_isPaused || _isDisabled) return;
            _isSwitchWeapon2Pressed = value.isPressed;
        }

        void OnStart(InputValue value) {
            if (!value.isPressed) return;
            if (_isDebug) {
                HideDebug();
                return;
            }
            if (_isPaused) {
                eventChannel.OnUnpause.Invoke();
            } else {
                eventChannel.OnPause.Invoke();
            }
        }

        void OnDebug(InputValue value) {
            _isDebug = !_isDebug;
            if (_isDebug) {
                ShowDebug();
            } else {
                HideDebug();
            }
        }

        void OnUIStart(InputValue value) {
            if (!value.isPressed) return;
            OnStart(value);
        }

        void OnUISelect(InputValue value) {
            if (_isDebug) {
                HideDebug();
            }
        }

        void OnAnyKey(InputValue value) {
            if (!value.isPressed) return;
            eventChannel.OnAnyKeyPress.Invoke();
        }

        // event callbacks

        void ShowDebug() {
            eventChannel.OnShowDebug.Invoke();
            input.SwitchCurrentActionMap("UI");
            _isDebug = true;
        }
        void HideDebug() {
            eventChannel.OnHideDebug.Invoke();
            input.SwitchCurrentActionMap("Player");
            _isDebug = false;
        }

        void OnPause() {
            input.SwitchCurrentActionMap("UI");
            _isPaused = true;
        }

        void OnUnpause() {
            input.SwitchCurrentActionMap("Player");
            _isPaused = false;
        }
    }
}
