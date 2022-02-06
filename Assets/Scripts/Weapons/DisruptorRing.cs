using UnityEngine;

using Damage;
using Game;
using Core;

namespace Weapons
{
    
    public class DisruptorRing : MonoBehaviour
    {
        DamageClass damageClass;

        void Start() {
            damageClass = GameManager.current.GetDamageClass(DamageType.Disruptor);
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.tag == UTag.Player) return;
            DamageReceiver actor = other.GetComponent<DamageReceiver>();
            if (actor == null) return;
            actor.TakeDamage(damageClass.baseDamage, DamageType.Disruptor);
        }
    }
}

