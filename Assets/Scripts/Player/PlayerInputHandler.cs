using UnityEngine;
using UnityEngine.InputSystem;

using Core;
using Event;

namespace Player {

    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] EventChannelSO eventChannel;
        [SerializeField] PlayerStateSO playerState;

        // components
        PlayerInput input;

        // cached
        Vector2 _move;
        Vector2 _look;
        Vector2 _touchPoint;
        bool _isFirePressed = false;
        bool _isFire2Pressed = false;
        bool _isMeleePressed = false;
        bool _isReloadPressed = false;
        bool _isBoostPressed = false;
        bool _isSwitchWeaponPressed = false;
        bool _isSwitchWeapon2Pressed = false;

        public Vector2 move => _move;
        public Vector2 look => _look;
        public bool isFirePressed => _isFirePressed;
        public bool isFire2Pressed => _isFire2Pressed;
        public bool isMeleePressed => _isMeleePressed;
        public bool isReloadPressed => _isReloadPressed;
        public bool isBoostPressed => _isBoostPressed;
        public bool isSwitchWeaponPressed => _isSwitchWeaponPressed;
        public bool isSwitchWeapon2Pressed => _isSwitchWeapon2Pressed;

        // state
        bool _isPaused = false;
        bool _isDebug = false;
        Transform _autoMoveTarget;

        public void SetMode(PlayerInputControlMode incoming) {
            if (playerState.controlMode != PlayerInputControlMode.Player && incoming == PlayerInputControlMode.Player) {
                _move = Vector2.zero;
            }
            playerState.SetInputControlMode(incoming);
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

        }

        void OnDisable() {
            eventChannel.OnPause.Unsubscribe(OnPause);
            eventChannel.OnUnpause.Unsubscribe(OnUnpause);
        }

        void Start() {
            AppIntegrity.AssertPresent(eventChannel);
            input = GetComponent<PlayerInput>();
            input.SwitchCurrentActionMap("Player");
            input.enabled = true;
        }

        void Update() {
            if (playerState.controlMode != PlayerInputControlMode.GameBrain) return;
            _move = (GetMoveTarget() - (Vector2)transform.position).normalized;
            if (Vector2.Distance(GetMoveTarget(), transform.position) < 0.2f) SetMode(PlayerInputControlMode.Player);
        }

        void OnMove(InputValue value) {
            if (playerState.controlMode == PlayerInputControlMode.GameBrain) return;
            if (playerState.controlMode != PlayerInputControlMode.Player) { _move = Vector2.zero; return; }
            if (_isPaused) return;
            _move = value.Get<Vector2>();
        }

        void OnLook(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { _look = Vector2.zero; return; }
            if (_isPaused) return;
            _look = value.Get<Vector2>();
        }

        void OnLookLeft(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { return; }
            if (_isPaused) return;
            if (!value.isPressed) { _look = Vector2.zero; return; }
            _look = new Vector2(-1f, _look.y).normalized;
        }

        void OnLookRight(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { return; }
            if (_isPaused) return;
            if (!value.isPressed) { _look = Vector2.zero; return; }
            _look = new Vector2(1f, _look.y).normalized;
        }

        void OnLookTouchscreen(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { return; }
            if (_isPaused) return;
            if (!value.isPressed) {
                return;
            }
            _touchPoint = value.Get<Vector2>();
            _look = _move = (_touchPoint - (Vector2)transform.position).normalized;
        }

        void OnFire(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { _isFirePressed = false; return; }
            if (_isPaused) return;
            _isFirePressed = value.isPressed;
        }

        void OnFireSecondary(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { _isFire2Pressed = false; return; }
            if (_isPaused) return;
            _isFire2Pressed = value.isPressed;
        }

        void OnMelee(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { _isMeleePressed = false; return; }
            if (_isPaused) return;
            _isMeleePressed = value.isPressed;
        }

        void OnReload(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { _isReloadPressed = false; return; }
            if (_isPaused) return;
            _isReloadPressed = value.isPressed;
        }

        void OnBoost(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { _isBoostPressed = false; return; }
            if (_isPaused) return;
            _isBoostPressed = value.isPressed;
        }

        void OnSwitchWeapon(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { _isSwitchWeaponPressed = false; return; }
            if (_isPaused) return;
            _isSwitchWeaponPressed = value.isPressed;
        }

        void OnSwitchSecondary(InputValue value) {
            if (playerState.controlMode != PlayerInputControlMode.Player) { _isSwitchWeapon2Pressed = false; return; }
            if (_isPaused) return;
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

        void OnUIAnyKey(InputValue value) {
            if (!value.isPressed) return;
            OnAnyKey(value);
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
