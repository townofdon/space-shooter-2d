using UnityEngine;

namespace Damage
{

    public class DamageReceiver : MonoBehaviour
    {
        // components
        DamageableBehaviour actor;
        Rigidbody2D rb;

        public new Rigidbody2D rigidbody => rb;
        public bool canCollide => actor != null ? actor.timeHit <= 0f : false;

        void Start() {
            rb = GetComponentInParent<Rigidbody2D>();
            actor = GetComponentInParent<DamageableBehaviour>();
        }

        public void TakeDamage(float amount, DamageType damageType = DamageType.Default) {
            if (actor != null) {
                actor.TakeDamage(amount, damageType);
            }
        }
    }
}
