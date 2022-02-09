using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float turnSpeed = 1f;
    [SerializeField] float lifetime = 10f;
    [SerializeField] Vector3 initialHeading = Vector3.up;

    // cached
    Rigidbody2D rb;
    Vector3 heading;
    Vector3 velocity;
    Transform target;

    // state
    float t = 0;

    public void SetTarget(Transform _target) {
        target = _target;
    }

    void Init() {
        heading = initialHeading;
        // point heading in direction of rotation
        heading = Quaternion.AngleAxis(transform.rotation.eulerAngles.z, Vector3.forward) * heading;
        velocity = heading * moveSpeed;
        target = null;
    }

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        Init();
    }

    void Update() {
        UpdateHeading();
        if (rb == null) MoveViaTransform();
        t += Time.deltaTime;
        if (t > lifetime) Destroy(gameObject);
    }

    void FixedUpdate() {
        if (rb != null) MoveViaRigidbody();
    }

    void UpdateHeading() {
        if (target != null) {
            heading = Vector3.RotateTowards(
                heading,
                (target.position - transform.position).normalized,
                turnSpeed * 2f * Mathf.PI * Time.fixedDeltaTime,
                1f
            ).normalized;
        }
        velocity = Vector3.MoveTowards(velocity, heading * moveSpeed, 2f * moveSpeed * Time.fixedDeltaTime);
    }

    void MoveViaTransform() {
        transform.position += velocity * Time.deltaTime;
    }

    void MoveViaRigidbody() {
        rb.velocity = velocity;
    }
}
