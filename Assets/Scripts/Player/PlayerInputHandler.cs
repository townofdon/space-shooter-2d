using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Player {

    public class PlayerInputHandler : MonoBehaviour
    {
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

        void OnMove(InputValue value) {
            _move = value.Get<Vector2>();
        }

        void OnLook(InputValue value) {
            _look = value.Get<Vector2>();
        }

        void OnFire(InputValue value) {
            _isFirePressed = value.isPressed;
        }

        void OnFireSecondary(InputValue value) {
            _isFire2Pressed = value.isPressed;
        }

        void OnMelee(InputValue value) {
            _isMeleePressed = value.isPressed;
        }

        void OnReload(InputValue value) {
            _isReloadPressed = value.isPressed;
        }

        void OnBoost(InputValue value) {
            _isBoostPressed = value.isPressed;
        }

        void OnSwitchWeapon(InputValue value) {
            _isSwitchWeaponPressed = value.isPressed;
        }

        void OnSwitchSecondary(InputValue value) {
            _isSwitchWeapon2Pressed = value.isPressed;
        }

        // TODO: REMOVE ACTION
        void OnReset(InputValue value) {
            if (value.isPressed) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        public bool isUpgradePressed = false;
        void OnTempUpgrade(InputValue value) {
            isUpgradePressed = value.isPressed;
        }
    }
}
