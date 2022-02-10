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
        bool _isBoostPressed = false;
        bool _isSwitchWeaponPressed = false;

        public Vector2 move => _move;
        public Vector2 look => _look;
        public bool isFirePressed => _isFirePressed;
        public bool isBoostPressed => _isBoostPressed;
        public bool isSwitchWeaponPressed => _isSwitchWeaponPressed;

        void OnMove(InputValue value) {
            _move = value.Get<Vector2>();
        }

        void OnLook(InputValue value) {
            _look = value.Get<Vector2>();
        }

        void OnFire(InputValue value) {
            _isFirePressed = value.isPressed;
        }

        void OnBoost(InputValue value) {
            _isBoostPressed = value.isPressed;
        }

        void OnSwitchWeapon(InputValue value) {
            _isSwitchWeaponPressed = value.isPressed;
        }


        // TODO: REMOVE ACTION
        void OnReset(InputValue value) {
            if (value.isPressed) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        // TODO: REMOVE ACTION
        public bool isDeadPressed = false;
        void OnDie(InputValue value) {
            isDeadPressed = value.isPressed;
        }
    }
}
