using UnityEngine;

public class ImpactVelocity : MonoBehaviour {
    [SerializeField] AnimationCurve curve;
    [SerializeField] float travelDistanceY = 1f;
    [SerializeField] float lifetime = 1f;

    Vector3 startPos;
    Vector3 destPos;
    float t;

    void Start() {
        startPos = transform.position;
        destPos = startPos + transform.up * travelDistanceY;
        t = 0;
    }

    void Update() {
        transform.position = Vector3.LerpUnclamped(startPos, destPos, curve.Evaluate(t / lifetime));
        t += Time.deltaTime;
        if (t > lifetime) {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
