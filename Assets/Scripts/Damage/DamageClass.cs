using UnityEngine;

namespace Damage
{
    public enum DamageType {
      Default,
      Collision,
      Laser,
      Bullet,
      Nuke,
      Disruptor,
    }

    [CreateAssetMenu(fileName = "DamageClass", menuName = "ScriptableObjects/DamageClass", order = 0)]
    public class DamageClass : ScriptableObject {
        [Header("Damage Settings")][Space]
        [SerializeField] DamageType _damageType = DamageType.Default;
        [SerializeField] float _baseDamage = 1f;
        [SerializeField][Range(0f, 100f)] float _damageVariance = 0f;
        [SerializeField][Range(0f, 1f)] float _shieldEffectiveness = 1f;
        [SerializeField][Range(0f, 1f)] float _shieldedHealthEffectiveness = 1f;
        [SerializeField][Range(0f, 1f)] float _unshieldedHealthEffectiveness = 0f;

        public DamageType damageType => _damageType;
        public float baseDamage => GetBaseDamage();
        public float shieldEffectiveness => _shieldEffectiveness;
        public float shieldedHealthEffectiveness => _shieldedHealthEffectiveness;
        public float unshieldedHealthEffectiveness => _unshieldedHealthEffectiveness;

        [Header("Actor Settings")][Space]
        [SerializeField][Range(0f, 1f)] float _playerEffectiveness = 1f;
        [SerializeField][Range(0f, 1f)] float _enemyEffectiveness = 1f;
        [SerializeField][Range(0f, 1f)] float _environmentEffectiveness = 1f;
        [SerializeField][Range(0f, 1f)] float _npcEffectiveness = 1f;

        public float playerEffectiveness => _playerEffectiveness;
        public float enemyEffectiveness => _enemyEffectiveness;
        public float environmentEffectiveness => _environmentEffectiveness;
        public float npcEffectiveness => _npcEffectiveness;

        public float GetBaseDamage() {
            return Mathf.Max(0f, _baseDamage + UnityEngine.Random.Range(-_damageVariance / 2, _damageVariance / 2));
        }

        // can add difficulty curve modifiers here as well
    }
}
