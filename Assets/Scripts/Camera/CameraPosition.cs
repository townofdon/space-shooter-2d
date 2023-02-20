using UnityEngine;

using Core;
using Player;

namespace CameraFX {

    public class CameraPosition : MonoBehaviour {
        [SerializeField] Vector3 _position;
        [SerializeField] Vector3 _offset;
        [SerializeField][Range(0f, 1f)] float horizontalTrackingMod = 0.1f;
        [SerializeField][Range(0f, 100f)] float trackingSpeed = 100f;
        [SerializeField] AnimationCurve trackingSpeedMod = AnimationCurve.Linear(0f, 0.1f, 1f, 1f);

        Camera _camera;
        Vector3 _initialPosition;
        Vector3 _targetPosition;
        PlayerGeneral _player;
        float _targetDelta;

        public Vector3 position { get => _position; }

        public void SetOffset(Vector3 value) {
            _offset = value;
        }

        void Awake() {
            _camera = Utils.GetCamera();
            _initialPosition = _camera.transform.position;
            _position = (Vector3)(Vector2)_initialPosition;
        }

        void Update() {
            MoveByPlayerPosition();
            _camera.transform.position = _initialPosition + _position + _offset;
        }

        void MoveByPlayerPosition() {
            if (!IsPlayerAlive()) _player = PlayerUtils.FindPlayer();
            if (IsPlayerAlive()) {
                _targetPosition.x = _player.transform.position.x * horizontalTrackingMod;
            }
            _targetDelta = Mathf.Clamp(Mathf.Abs(_position.x - _targetPosition.x) * 2f, 0f, 1f);
            _position = Vector3.MoveTowards(_position, _targetPosition, trackingSpeed * trackingSpeedMod.Evaluate(_targetDelta) * Time.deltaTime);
        }

        bool IsPlayerAlive() {
            return _player != null && _player.isActiveAndEnabled && _player.isAlive;
        }
    }
}
