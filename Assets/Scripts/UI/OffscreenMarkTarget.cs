using UnityEngine;

using Damage;

namespace UI {

    public class OffscreenMarkTarget : MonoBehaviour {
        [SerializeField] GameObject markerPrefab;
        [SerializeField] FlagType flagType;

        // cached
        GameObject instance;
        OffscreenMarker marker;

        DamageableBehaviour actor;

        void Start() {
            if (markerPrefab == null) return;
            instance = Instantiate(markerPrefab, Vector3.zero, Quaternion.identity);
            marker = instance.GetComponent<OffscreenMarker>();
            if (marker == null) return;
            marker.SetTarget(transform);
            marker.SetFlagType(flagType);
            actor = GetComponent<DamageableBehaviour>();
        }

        void Update() {
            if (actor == null || !actor.isAlive) Deactivate();
        }

        void Deactivate() {
            if (marker != null) marker.Disable();
        }

        void OnDestroy() {
            if (marker != null) Destroy(marker.gameObject);
        }
    }
}
