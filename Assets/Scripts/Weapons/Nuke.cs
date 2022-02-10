using System.Collections;
using UnityEngine;
using Core;
using Audio;

namespace Weapons
{

    public class Nuke : MonoBehaviour
    {
        [SerializeField] float fuseTime = 3f;
        [SerializeField] float finalAnimSpeed = 10f;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] Vector2 defaultDirection = Vector2.up;
        [SerializeField] GameObject nukeExplosion;
        [SerializeField] Gradient lifetimeColor;

        [Header("Audio")][Space]
        [SerializeField] Sound anticipationSound;
        [SerializeField] Sound explosionSound;

        // cached
        Rigidbody2D rb;
        SpriteRenderer sr;
        Animator anim;

        // state
        Vector2 direction = Vector2.up;
        float currentTime = 0f;
        bool sploded = false;
        GameObject splosion;

        void Start() {
            AppIntegrity.AssertPresent<GameObject>(nukeExplosion);
            rb = Utils.GetRequiredComponent<Rigidbody2D>(gameObject);
            sr = Utils.GetRequiredComponent<SpriteRenderer>(gameObject);
            anim = Utils.GetRequiredComponent<Animator>(gameObject);
            // init
            currentTime = 0f;
            sr.enabled = true;
            direction = defaultDirection;

            anticipationSound.Init(this);
            explosionSound.Init(this);
            anticipationSound.Play();
        }

        void Update() {
            rb.velocity = Vector2.Lerp(direction.normalized * moveSpeed, Vector2.zero, EasedTime());
            anim.speed = Mathf.Lerp(1f, finalAnimSpeed, EasedTime());
            sr.color = lifetimeColor.Evaluate(EasedTime());

            if (currentTime >= fuseTime) {
                if (!sploded) StartCoroutine(Splode());
            } else {
                currentTime = Mathf.Clamp(currentTime + Time.deltaTime, 0f, fuseTime);
            }
        }

        float EasedTime() {
            return Mathf.Clamp(Easing.easeInExpo(currentTime / fuseTime), 0f, 1f);
        }

        IEnumerator Splode() {
            if (sploded) yield return null;
            sploded = true;
            sr.enabled = false;
            explosionSound.Play();
            splosion = Object.Instantiate(nukeExplosion, transform.position, new Quaternion(0f,0f,0f,0f));
            // TODO: USE OBJECT POOLING SYSTEM
            Destroy(splosion, 7f);
            Destroy(gameObject, 7f);
        }

        public void SetDirection(Vector2 _direction) {
            direction = _direction.magnitude > 0.1f
                ? _direction.normalized
                : defaultDirection;
        }
    }
}

