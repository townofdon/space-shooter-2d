
using UnityEngine;
using Core;

namespace Physics {
    public class NukeSweepProjectile : MonoBehaviour {

        [SerializeField] float edgeFudge = 0.25f;

        Rigidbody2D rb;
        Transform shockwaveOrigin;
        Vector3 heading;
        float shockwaveRadius = 1f;
        bool isSweeping;

        void Start() {
            rb = GetComponentInParent<Rigidbody2D>();
        }

        // void FixedUpdate() {
        //     if (!isSweeping) return;
        //     if (shockwaveOrigin == null) return;
        //     shockwaveRadius = (shockwaveOrigin.lossyScale.x / 2f);
        //     transform.position = shockwaveOrigin.position + heading * shockwaveRadius + heading * edgeFudge;
        // }

        // void OnTriggerEnter2D(Collider2D other) {
        //     if (isSweeping) return;
        //     if (!Utils.IsObjectOnScreen(gameObject)) return;
        //     if (other.tag == UTag.NukeShockwave) {
        //         shockwaveOrigin = other.transform;
        //         if ((transform.position - shockwaveOrigin.position).magnitude < shockwaveOrigin.lossyScale.x / 2f - edgeFudge) return;
        //         heading = (transform.position - shockwaveOrigin.position).normalized;
        //         isSweeping = true;
        //         if (rb != null) rb.isKinematic = true;
        //         return;
        //     }
        // }
    }
}

