using UnityEngine;

namespace UI {

    public class OffscreenMarkTarget : MonoBehaviour {
        [SerializeField] GameObject markerPrefab;
        [SerializeField] FlagType flagType;

        // cached
        GameObject instance;
        OffscreenMarker marker;

        void Start() {
            if (markerPrefab == null) return;
            instance = Instantiate(markerPrefab, Vector3.zero, Quaternion.identity);
            marker = instance.GetComponent<OffscreenMarker>();
            if (marker == null) return;
            marker.SetTarget(transform);
            marker.SetFlagType(flagType);
        }

        void OnDestroy() {
            if (marker != null) Destroy(marker.gameObject);
        }
    }
}
