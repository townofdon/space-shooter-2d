using UnityEngine;

namespace Damage {

    public class LifetimeDestroyer : MonoBehaviour {
        [SerializeField] float lifetime = 20f;

        DamageableBehaviour actor;
        float t;

        void Start() {
            actor = GetComponentInParent<DamageableBehaviour>();
        }

        void Update() {
            if (t >= lifetime) {
                if (actor != null) {
                    actor.TakeDamage(10000f, DamageType.InstakillQuiet);
                } else {
                    Destroy(gameObject);
                }
            }
            t += Time.deltaTime;
        }
    }
}
