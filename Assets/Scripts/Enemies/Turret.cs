using System.Collections;
using Audio;
using UnityEngine;

namespace Enemies {

    public class Turret : MonoBehaviour {
        [SerializeField] float activationDelay = 1f;
        [SerializeField] bool animateDeploy;

        [Header("Animations")]
        [SerializeField] Animator deployAnimator;

        [Header("Audio")]
        [Space]
        [SerializeField] Sound deploySound;
        [SerializeField] Sound activateSound;
        [SerializeField] Sound retractSound;

        EnemyShip enemy;
        EnemyShooter shooter;

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
        }

        void Start() {
            activateSound.Init(this);
            deploySound.Init(this);
            retractSound.Init(this);
            StartCoroutine(IDeploy());
        }

        void OnDeath() {
            if (deployAnimator != null) {
                deployAnimator.SetTrigger("Retract");
                StartCoroutine(IRetract());
            }
        }

        IEnumerator IRetract() {
            yield return new WaitForSeconds(0.25f);
            retractSound.Play();
        }

        IEnumerator IDeploy() {
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));
            if (deployAnimator != null && deployAnimator.enabled) {
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
            if (shooter != null) shooter.enabled = true;
        }
    }
}
