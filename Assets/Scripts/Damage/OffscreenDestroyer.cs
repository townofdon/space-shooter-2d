using UnityEngine;

using Core;

namespace Damage
{

    public class OffscreenDestroyer : MonoBehaviour
    {
        [SerializeField] float offscreenLifetime = 10f;
        [SerializeField] float outsideBoundsPadding = 1f;

        [Space]

        [SerializeField] bool killNorth = true;
        [SerializeField] bool killSouth = true;
        [SerializeField] bool killEast = true;
        [SerializeField] bool killWest = true;

        // components
        Rigidbody2D rb;
        DamageableBehaviour actor;

        // state
        float timeOutsideBounds = 0f;
        Vector2 minBounds;
        Vector2 maxBounds;

        void Start() {
            rb = GetComponentInParent<Rigidbody2D>();
            actor = GetComponentInParent<DamageableBehaviour>();
            // init
            timeOutsideBounds = 0f;
            minBounds = Utils.GetCamera().ViewportToWorldPoint(new Vector2(0f, 0f)) - (Vector3)Vector2.one * outsideBoundsPadding;
            maxBounds = Utils.GetCamera().ViewportToWorldPoint(new Vector2(1f, 1f)) + (Vector3)Vector2.one * outsideBoundsPadding;
        }

        void Update() {
            if (
                (killWest && transform.position.x < minBounds.x) ||
                (killEast && transform.position.x > maxBounds.x) ||
                (killNorth && transform.position.y > maxBounds.y) ||
                (killSouth && transform.position.y < minBounds.y)
            ) {
                timeOutsideBounds += Time.deltaTime;
            } else {
                timeOutsideBounds = 0f;
            }
            if (timeOutsideBounds >= offscreenLifetime) {
                if (actor != null) {
                    actor.TakeDamage(10000f, DamageType.InstakillQuiet);
                } else {
                    Destroy(gameObject);
                }
            }
        }
    }
}
