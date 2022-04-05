using Enemies;
using UnityEngine;

namespace UI {

    public class OffscreenMarkTarget : MonoBehaviour {
        [SerializeField] GameObject markerPrefab;
        [SerializeField] FlagType flagType;

        // cached
        GameObject instance;
        OffscreenMarker marker;

        EnemyShip enemy;

        void Start() {
            if (markerPrefab == null) return;
            instance = Instantiate(markerPrefab, Vector3.zero, Quaternion.identity);
            marker = instance.GetComponent<OffscreenMarker>();
            if (marker == null) return;
            marker.SetTarget(transform);
            marker.SetFlagType(flagType);
            enemy = GetComponent<EnemyShip>();
        }

        void Update() {
            if (enemy == null || !enemy.isAlive) Deactivate();
        }

        void Deactivate() {
            if (marker != null) marker.Disable();
        }

        void OnDestroy() {
            if (marker != null) Destroy(marker.gameObject);
        }
    }
}
