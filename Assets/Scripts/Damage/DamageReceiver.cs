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
        public System.Nullable<System.Guid> uuid => actor != null ? actor.uuid : null;

        void Start() {
            rb = GetComponentInParent<Rigidbody2D>();
            actor = GetComponentInParent<DamageableBehaviour>();
        }

        public bool TakeDamage(float amount, DamageType damageType = DamageType.Default) {
            if (actor == null) return false;
            return actor.TakeDamage(amount, damageType);
        }

        public bool DrainShield(float amount) {
            if (actor == null) return false;
            return actor.DrainShield(amount);
        }
    }
}
