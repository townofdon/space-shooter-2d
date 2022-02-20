using UnityEngine;

using Core;
using Audio;
using Damage;

public class ShieldFX : MonoBehaviour
{
    [Header("ShieldFX")][Space]
    [SerializeField] ParticleSystem shieldIdleFX;
    [SerializeField] ParticleSystem shieldHitFX;
    [SerializeField] ParticleSystem shieldLostEffect;
    [SerializeField] LoopableSound shieldSound;
    [SerializeField] Sound shieldLostSound;

    // components
    DamageableBehaviour damageableBehaviour;

    // state
    Timer shieldHit = new Timer();

    void OnEnable() {
        if (damageableBehaviour == null) return;
        damageableBehaviour.OnDeathEvent += OnDeath;
        damageableBehaviour.OnShieldHitEvent += OnShieldHit;
        damageableBehaviour.OnShieldDepletedEvent += OnShieldDepleted;
    }

    void OnDisable() {
        if (damageableBehaviour == null) return;
        damageableBehaviour.OnDeathEvent -= OnDeath;
        damageableBehaviour.OnShieldHitEvent -= OnShieldHit;
        damageableBehaviour.OnShieldDepletedEvent -= OnShieldDepleted;
    }

    void Awake()
    {
        damageableBehaviour = GetComponent<DamageableBehaviour>();
        shieldSound.Init(this);
        shieldLostSound.Init(this);
        shieldHit.SetDuration(0.2f);
    }

    void Update()
    {
        HandleShields();
        shieldHit.Tick();
    }

    void HandleShields() {
        if (damageableBehaviour.isAlive && damageableBehaviour.shield > 0f) {
            if (damageableBehaviour.timeHit > 0f || shieldHit.active) {
                ShowShieldHit();
                shieldSound.Play();
            } else {
                ShowShieldIdle();
                shieldSound.Stop();
            }
        } else {
            ShowNoShield();
        }
    }

    void ShowShieldHit() {
        if (shieldHitFX != null && !shieldHitFX.isPlaying) shieldHitFX.Play();
        if (shieldIdleFX != null) shieldIdleFX.Stop();
    }

    void ShowShieldIdle() {
        if (shieldIdleFX != null && !shieldIdleFX.isPlaying) shieldIdleFX.Play();
        if (shieldHitFX != null) shieldHitFX.Stop();
    }

    void ShowNoShield() {
        if (shieldIdleFX != null) shieldIdleFX.Stop();
        if (shieldHitFX != null) shieldHitFX.Stop();
        shieldSound.Stop();
    }

    void OnShieldHit() {
        shieldHit.Start();
    }

    void OnShieldDepleted() {
        if (shieldLostEffect != null && !shieldLostEffect.isPlaying) shieldLostEffect.Play();
        shieldLostSound.Play();
        shieldSound.Stop();
    }

    void OnDeath() {
        ShowNoShield();
    }
}
