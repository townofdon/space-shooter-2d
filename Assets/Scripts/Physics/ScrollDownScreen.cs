
using UnityEngine;

using Core;

namespace Physics {

    public class ScrollDownScreen : MonoBehaviour {
        [SerializeField] bool debug = false;
        [SerializeField][Range(0f, 50f)] float moveSpeed = 5f;
        [SerializeField][Range(0f, 50f)] float speedVariance = 3f;
        [SerializeField][Range(0f, 50f)] float accel = 1f;

        Rigidbody2D rb;

        void Start() {
            moveSpeed = Utils.RandomVariance(moveSpeed, speedVariance, moveSpeed / 2f);
            rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.down * moveSpeed;
        }

        void Update() {
            if (rb != null) return;
            transform.position = transform.position + (Vector3)Vector2.down * moveSpeed * Time.deltaTime;
        }

        void FixedUpdate() {
            if (rb == null) return;
            if (debug) Debug.Log(rb.velocity.y);

            if (rb.velocity.y > -moveSpeed) rb.AddForce(Vector2.down * accel);
        }
    }
}
