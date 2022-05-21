using UnityEngine;

using Damage;

namespace UI {

    public class OffscreenMarkTarget : MonoBehaviour {
        [SerializeField] GameObject markerPrefab;
        [SerializeField] FlagType flagType;

        [Space]

        [SerializeField] bool showNorth = true;
        [SerializeField] bool showSouth = true;
        [SerializeField] bool showEast = true;
        [SerializeField] bool showWest = true;

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
            marker.SetOrdinals(showNorth, showEast, showSouth, showWest);
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
