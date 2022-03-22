using UnityEngine;

using Core;

namespace Physics {

    public struct RaycastCheck {
        int _instanceId;
        Vector2 _initialPosition;
        Vector2 _origin;
        Vector2 _heading;
        float _closeness; // 1 = max; 0 = min
        float _angle;
        RaycastHit2D[] _hits;
        int _numHits;
        bool _didHit;
        ULayerType _layerType;
        Timer _didHitTimer;

        public int instanceId => _instanceId;
        public Vector2 initialPosition => _initialPosition;
        public Vector2 heading => _heading;
        public float angle => _angle;
        public float closeness => _closeness;
        public RaycastHit2D hit => _hits[0];
        public bool didHit => _didHit && _numHits > 0 && _hits.Length > 0 && _hits[0].collider != null;
        public ULayerType layerType => _layerType;

        public RaycastCheck(Vector2 initialPosition, float headingAngle, int instanceId, int selfLayerMask) {
            _instanceId = instanceId;
            _initialPosition = initialPosition;
            _origin = Vector2.zero;
            _closeness = 0f;
            _angle = headingAngle;
            _heading = Quaternion.AngleAxis(headingAngle, Vector3.forward) * Vector2.down;
            _hits = new RaycastHit2D[1];
            _layerType = ULayerType.Default;
            _numHits = 0;
            _didHit = false;
            _didHitTimer = new Timer(TimerDirection.Decrement, TimerStep.FixedDeltaTime);
            _didHitTimer.SetDuration(0.5f);
            CalcOrigin(instanceId, selfLayerMask);
        }

        public RaycastHit2D CheckForHit(Vector2 position, float raycastDist, float adjustedAngle, int layerMask) {
            _numHits = Physics2D.LinecastNonAlloc(
                position + _origin,
                position + _origin + (Vector2)(Quaternion.AngleAxis(adjustedAngle, Vector3.forward) * _heading * raycastDist),
                _hits,
                layerMask);
            // NOTE - for some reason raycasts hit self even after setting the origin outside of all colliders. I think maybe some drift is occurring.
            _didHit = _numHits > 0 && Utils.GetRootInstanceId(_hits[0].collider.gameObject) != _instanceId;
            if (didHit) {
                _layerType = ULayer.Lookup(_hits[0].collider.gameObject.layer).layerType;
                // convert [max-min] to inverse [0-1] range, where max => 0 and min => 1
                _closeness = 1f - (Mathf.Clamp(_hits[0].distance, _origin.magnitude, raycastDist) - _origin.magnitude) / (raycastDist - _origin.magnitude);
                _didHitTimer.Start();
            }
            return _hits[0];
        }

        public void FixedTick() {
            _didHitTimer.Tick();
            if (_didHitTimer.tEnd) Reset();
        }

        public void OnDebug(Vector2 position, float distance = 1f, float adjustedAngle = 0f) {
            Gizmos.color = (_didHit || _didHitTimer.active) ? Color.red : Color.cyan;
            Gizmos.DrawLine(position + _origin, position + _origin + (Vector2)(Quaternion.AngleAxis(adjustedAngle, Vector3.forward) * _heading * distance));
        }

        void Reset() {
            _didHit = false;
            _numHits = 0;
        }

        void CalcOrigin(int instanceId, int selfLayerMask) {
            if (instanceId == 0) return;
            RaycastHit2D hit;
            int safeguard = 100;
            // continue raycasting until we don't hit something
            do {
                // hit = Physics2D.Raycast(_initialPosition + _origin, _heading, 5f, selfLayerMask);
                hit = Physics2D.Linecast(_initialPosition + _origin, _initialPosition + _origin + _heading * 5f, selfLayerMask);
                if (hit.collider != null && Utils.GetRootInstanceId(hit.collider.gameObject) == instanceId) {
                    _origin = _origin + hit.distance * _heading + _heading * 0.11f;
                }
                safeguard--;
                if (safeguard == 0) Debug.LogWarning("INFINITE LOOP AVERTED");
            } while (hit.collider != null && _origin.magnitude < 5f && safeguard > 0);

            _didHit = false;
        }

        public static bool operator ==(RaycastCheck c1, RaycastCheck c2) {
            return c1.Equals(c2);
        }

        public static bool operator !=(RaycastCheck c1, RaycastCheck c2) {
            return !c1.Equals(c2);
        }

        public bool Equals(RaycastCheck rc) {
            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, rc)) {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != rc.GetType()) {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            // return (X == rc.X) && (Y == rc.Y);
            return
                _instanceId == rc.instanceId &&
                _initialPosition == rc.initialPosition &&
                _heading == rc.heading &&
                _closeness == rc.closeness &&
                _angle == rc._angle;
        }
    }
}
