using Core;
using UnityEngine;

namespace CameraFX {


    public class Parallax : MonoBehaviour {
        [SerializeField][Range(0f, 1f)] float parallaxMod = 1f;

        Vector3 position;
        Vector3 offset;

        private void Start() {
            position = transform.position;
            TurnOffIfDynamicRigidBody();
        }

        void Update() {
            // negate previous offset
            position = transform.position - offset;
            offset.x = Utils.GetCameraPosition().x * GetZMod() * parallaxMod;
            transform.position = position + offset;
        }

        void TurnOffIfDynamicRigidBody() {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null && rb.simulated && !rb.isKinematic) {
                this.enabled = false;
            }
        }

        // if z === 10 => return 1
        // if z === 0 => return 0
        // if z === -10 => return -1
        public float GetZMod() {
            return Mathf.Clamp(transform.position.z * 0.1f, -1f, 1f);
        }
    }
}
