using UnityEngine;

using Core;

namespace Physics {

    public class RotateOnStart : MonoBehaviour {
        [SerializeField][Range(0f, 1080f)] float startRotation = 60f;
        [SerializeField][Range(0f, 10f)] float startTranslate = 0.1f;

        Rigidbody2D rb;

        void Start() {
            rb = GetComponent<Rigidbody2D>();
            rb.angularVelocity = UnityEngine.Random.Range(-startRotation, startRotation);
            rb.AddForce(Utils.RandomVector2() * startTranslate, ForceMode2D.Impulse);
        }
    }
}
