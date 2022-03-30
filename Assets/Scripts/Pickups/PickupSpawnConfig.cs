using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pickups {

    [System.Serializable]
    public class PickupSpawnConfig {
        [SerializeField] GameObject pickupPrefab;
        [SerializeField][Range(1, 100)] int numItemsToSpawn = 1;
        [SerializeField][Range(0f, 1f)] float spawnLikelihood;

        // cache
        Rigidbody2D rbCurrent;

        float num = 0;
        int step = 0;
        bool didSpawn = false;

        public bool Spawn(Vector3 position, Rigidbody2D rbParent = null) {
            if (pickupPrefab == null) return false;
            didSpawn = false;
            for (step = 0; step < numItemsToSpawn; step++) {
                num = UnityEngine.Random.Range(0f, 1f);
                if (num > spawnLikelihood) continue;
                GameObject item = GameObject.Instantiate(pickupPrefab, position, Quaternion.identity);
                if (rbParent != null) SetVelocity(rbParent, item);
                didSpawn = true;
            }
            return didSpawn;
        }

        void SetVelocity(Rigidbody2D rbParent, GameObject item) {
            if (rbParent == null) return;
            if (item == null) return;
            rbCurrent = item.GetComponent<Rigidbody2D>();
            if (rbCurrent == null) return;
            rbCurrent.velocity = rbParent.velocity;
        }
    }

    [System.Serializable]
    public class PickupsSpawnConfig {
        [SerializeField] List<PickupSpawnConfig> pickups;

        public void Spawn(Vector3 position, Rigidbody2D rbParent = null) {
            foreach (var pickup in pickups) {
                if (pickup.Spawn(position, rbParent)) break;
            }
        }
    }
}

