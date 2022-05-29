
using UnityEngine;
using Core;
using Audio;

namespace Weapons
{

    public class Nuke : MonoBehaviour
    {
        [SerializeField] bool isDamageByPlayer = false;
        [SerializeField] float fuseTime = 3f;
        [SerializeField] float finalAnimSpeed = 10f;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] Vector2 defaultHeading = Vector2.up;
        [SerializeField] GameObject nukeExplosion;
        [SerializeField] Gradient lifetimeColor;

        [Header("Audio")][Space]
        [SerializeField] Sound anticipationSound;
        [SerializeField] Sound explosionSound;

        // cached
        Rigidbody2D rb;
        SpriteRenderer sr;
        SpriteRenderer[] renderers;
        Animator anim;

        // state
        Vector2 heading = Vector2.up;
        float currentTime = 0f;
        bool sploded = false;

        void Start() {
            AppIntegrity.AssertPresent<GameObject>(nukeExplosion);
            rb = Utils.GetRequiredComponent<Rigidbody2D>(gameObject);
            sr = Utils.GetRequiredComponent<SpriteRenderer>(gameObject);
            anim = Utils.GetRequiredComponent<Animator>(gameObject);
            renderers = GetComponentsInChildren<SpriteRenderer>();
            // init
            currentTime = 0f;
            sr.enabled = true;
            heading = defaultHeading;
            heading = Quaternion.AngleAxis(transform.rotation.eulerAngles.z, Vector3.forward) * heading;

            anticipationSound.Init(this);
            explosionSound.Init(this);
            anticipationSound.Play();
        }

        void Update() {
            rb.velocity = Vector2.Lerp(heading.normalized * moveSpeed, Vector2.zero, EasedTime());
            anim.speed = Mathf.Lerp(1f, finalAnimSpeed, EasedTime());
            sr.color = lifetimeColor.Evaluate(EasedTime());

            if (currentTime >= fuseTime) {
                Explode();
            } else {
                currentTime = Mathf.Clamp(currentTime + Time.deltaTime, 0f, fuseTime);
            }
        }

        float EasedTime() {
            return Mathf.Clamp(Easing.easeInExpo(currentTime / fuseTime), 0f, 1f);
        }

        public void Explode() {
            if (sploded) return;
            sploded = true;
            if (sr != null) sr.enabled = false;
            if (renderers != null) foreach (SpriteRenderer renderer in renderers) renderer.enabled = false;
            explosionSound.Play();
            GameObject instance = Object.Instantiate(nukeExplosion, transform.position, Quaternion.identity);
            NukeExplosion splosion = instance.GetComponent<NukeExplosion>();
            if (splosion != null) splosion.SetIsDamageByPlayer(isDamageByPlayer);
            // TODO: USE OBJECT POOLING SYSTEM
            Destroy(instance, 7f);
            Destroy(gameObject, 7f);
        }

        public void SetDirection(Vector2 _direction) {
            heading = _direction.magnitude > 0.1f
                ? _direction.normalized
                : defaultHeading;
        }
    }
}

