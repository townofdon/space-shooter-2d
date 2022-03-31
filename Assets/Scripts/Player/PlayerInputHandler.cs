using UnityEngine;
using UnityEngine.InputSystem;

using Core;
using Event;

namespace Player {

    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] EventChannelSO eventChannel;

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

        void Start() {
            AppIntegrity.AssertPresent(eventChannel);
        }

        void OnMove(InputValue value) {
            if (_isPaused) return;
            _move = value.Get<Vector2>();
        }

        void OnLook(InputValue value) {
            if (_isPaused) return;
            _look = value.Get<Vector2>();
        }

        void OnFire(InputValue value) {
            if (_isPaused) return;
            _isFirePressed = value.isPressed;
        }

        void OnFireSecondary(InputValue value) {
            if (_isPaused) return;
            _isFire2Pressed = value.isPressed;
        }

        void OnMelee(InputValue value) {
            if (_isPaused) return;
            _isMeleePressed = value.isPressed;
        }

        void OnReload(InputValue value) {
            if (_isPaused) return;
            _isReloadPressed = value.isPressed;
        }

        void OnBoost(InputValue value) {
            if (_isPaused) return;
            _isBoostPressed = value.isPressed;
        }

        void OnSwitchWeapon(InputValue value) {
            if (_isPaused) return;
            _isSwitchWeaponPressed = value.isPressed;
        }

        void OnSwitchSecondary(InputValue value) {
            if (_isPaused) return;
            _isSwitchWeapon2Pressed = value.isPressed;
        }

        void OnStart(InputValue value) {
            if (!value.isPressed) return;
            _isPaused = !_isPaused;
            if (_isPaused) {
                eventChannel.OnPause.Invoke();
            } else {
                eventChannel.OnUnpause.Invoke();
            }

        }

        // TODO: REMOVE
        public bool isUpgradePressed = false;
        void OnTempUpgrade(InputValue value) {
            isUpgradePressed = value.isPressed;
        }
    }
}
