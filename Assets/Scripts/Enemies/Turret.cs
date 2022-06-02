using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Audio;
using Game;
using Core;
using Damage;

namespace Enemies {

    public class Turret : MonoBehaviour {
        [SerializeField] float activationDelay = 1f;
        [SerializeField] bool animateDeploy;
        [SerializeField] bool animateSpawn;

        [Header("Animations")]
        [SerializeField] Animator deployAnimator;

        [Header("Spawn")]
        [SerializeField] Gradient spawnGradient;
        [SerializeField] ParticleSystem spawnFX;
        [SerializeField] List<SpriteRenderer> spawnSprites = new List<SpriteRenderer>();
        [SerializeField] Material defaultMaterial;
        [SerializeField] Material spawnMaterial;

        [Header("Death")]
        [SerializeField] List<SpriteRenderer> deathFadeOutSprites = new List<SpriteRenderer>();
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeOutDelay = 1f;

        [Header("Audio")]
        [Space]
        [SerializeField] Sound spawnSound;
        [SerializeField] Sound deploySound;
        [SerializeField] Sound activateSound;
        [SerializeField] Sound retractSound;

        EnemyShip enemy;
        EnemyShooter shooter;

        Timer spawning = new Timer(TimerDirection.Increment);
        Timer fadeout = new Timer(TimerDirection.Decrement);
        ParticleSystem.MainModule spawnFXModule;
        Dictionary<SpriteRenderer, Color> spriteColourMap = new Dictionary<SpriteRenderer, Color>();

        void OnEnable() {
            enemy.OnDeathEvent += OnDeath;
        }

        void OnDisable() {
            enemy.OnDeathEvent -= OnDeath;
        }

        void Awake() {
            enemy = GetComponent<EnemyShip>();
            shooter = GetComponent<EnemyShooter>();
            if (deployAnimator != null && animateDeploy) {
                deployAnimator.enabled = true;
                deployAnimator.speed = 0f;
            }
            if (spawnFX != null) spawnFXModule = spawnFX.main;
            spawning.SetDuration(activationDelay);
            if (animateSpawn) {
                foreach (var sprite in spawnSprites) {
                    spriteColourMap.Add(sprite, sprite.color);
                    sprite.color = spawnGradient.Evaluate(0);
                }
            }
        }

        void Start() {
            if (enemy != null && GameManager.current.difficulty >= GameDifficulty.Hard) enemy.SetInvulnerable(true);
            activateSound.Init(this);
            spawnSound.Init(this);
            deploySound.Init(this);
            retractSound.Init(this);
            StartCoroutine(IDeploy());
            StartCoroutine(ISpawn());
        }

        void OnDeath(DamageType damageType, bool isDamageByPlayer) {
            if (deployAnimator != null && animateDeploy) {
                deployAnimator.SetTrigger("Retract");
                StartCoroutine(IRetract());
            }
            if (deathFadeOutSprites.Count > 0) {
                StartCoroutine(IFadeOut());
            }
        }

        IEnumerator IRetract() {
            yield return new WaitForSeconds(0.25f);
            retractSound.Play();
        }

        IEnumerator IDeploy() {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));
            if (deployAnimator != null && deployAnimator.enabled && animateDeploy) {
                deploySound.Play();
                deployAnimator.speed = 1f;
            }
            yield return IActivate();
        }

        IEnumerator IActivate() {
            yield return new WaitForSeconds(activationDelay);
            while (deploySound.isPlaying) yield return null;
            activateSound.Play();
            while (activateSound.isPlaying) yield return null;
            if (enemy != null) enemy.SetInvulnerable(false);
            if (shooter != null) shooter.enabled = true;
        }

        IEnumerator ISpawn() {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));
            if (animateSpawn && spawnSprites.Count > 0) {
                spawnSound.Play();
                foreach (var sprite in spawnSprites) sprite.material = spawnMaterial;
                spawnFX.Play();
                spawning.Start();
            }

            while (spawning.active) {
                spawning.Tick();
                if (spawnFX != null) spawnFXModule.simulationSpeed = Mathf.Lerp(0.5f, 2f, spawning.value);
                foreach (var sprite in spawnSprites) sprite.color = spawnGradient.Evaluate(spawning.value);
                yield return null;
            }

            spawnSound.Stop();

            if (animateSpawn && spawnSprites.Count > 0) {
                foreach (var sprite in spawnSprites) {
                    sprite.material = defaultMaterial;
                    sprite.color = spriteColourMap[sprite];
                }
                spawnFX.Stop();
            }
        }

        IEnumerator IFadeOut() {
            yield return new WaitForSeconds(fadeOutDelay);
            fadeout.SetDuration(fadeOutTime);
            fadeout.Start();
            while (fadeout.active) {
                fadeout.Tick();
                foreach (var sprite in deathFadeOutSprites) sprite.color = spawnGradient.Evaluate(fadeout.value);
                yield return null;
            }
        }
    }
}
