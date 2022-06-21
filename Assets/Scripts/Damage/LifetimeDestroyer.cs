using UnityEngine;

namespace Damage {

    public class LifetimeDestroyer : MonoBehaviour {
        [SerializeField] float lifetime = 20f;

        DamageableBehaviour actor;
        ParticleSystem particles;
        float t;

        bool destroying = false;

        void Start() {
            actor = GetComponent<DamageableBehaviour>();
            particles = GetComponentInParent<ParticleSystem>();
        }

        void Update() {
            if (destroying) return;
            if (t >= lifetime) {
                destroying = true;
                if (actor != null) {
                    actor.TakeDamage(10000f, DamageType.InstakillQuiet);
                } else if (particles != null) {
                    particles.Stop();
                    Destroy(gameObject, 10f);
                } else {
                    Destroy(gameObject);
                }
            }
            t += Time.deltaTime;
        }
    }
}
