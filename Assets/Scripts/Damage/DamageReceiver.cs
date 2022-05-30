using UnityEngine;

namespace Damage
{

    public class DamageReceiver : MonoBehaviour
    {
        // components
        DamageableBehaviour actor;
        Rigidbody2D rb;

        public DamageableBehaviour root => actor;
        public new Rigidbody2D rigidbody => rb;
        public bool isAlive => actor != null && actor.isAlive;
        public bool canCollide => actor != null ? actor.timeHit <= 0f : false;
        public DamageableType damageableType => actor != null ? actor.damageableType : DamageableType.Default;
        public System.Nullable<System.Guid> uuid => actor != null ? actor.uuid : null;

        void Start() {
            rb = GetComponentInParent<Rigidbody2D>();
            actor = GetComponentInParent<DamageableBehaviour>();
        }

        public bool TakeDamage(float amount, DamageType damageType = DamageType.Default, bool isDamageByPlayer = false) {
            if (actor == null) return false;
            return actor.TakeDamage(amount, damageType, isDamageByPlayer);
        }

        public bool DrainShield(float amount) {
            if (actor == null) return false;
            return actor.DrainShield(amount);
        }

        public void TakeImpactForce(Vector3 incomingVelocity, float incomingMass, float throwbackForceMultiplier = 0f) {
            if (actor == null || rb == null) return;
            rb.AddForce(incomingVelocity * incomingMass * throwbackForceMultiplier, ForceMode2D.Impulse);
        }
    }
}
