using UnityEngine;

namespace UI {

    public class OffscreenMarkTarget : MonoBehaviour {
        [SerializeField] GameObject markerPrefab;
        [SerializeField] FlagType flagType;

        // cached
        OffscreenMarker marker;

        void Start() {
            if (markerPrefab == null) return;
            Instantiate(markerPrefab, transform.position, Quaternion.identity, transform);
            marker = markerPrefab.GetComponent<OffscreenMarker>();
            if (marker == null) return;
            marker.SetTarget(transform);
            marker.SetFlagType(flagType);
        }
    }
}
