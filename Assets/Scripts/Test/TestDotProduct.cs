using UnityEngine;

public class TestDotProduct : MonoBehaviour {
    [SerializeField] Transform target;

    void Update() {
        if (target == null) return;
        // Debug.Log(Vector2.Dot(transform.up, (transform.position - target.position).normalized));

        float distanceMod = 1 - Mathf.InverseLerp(2, 10, Vector2.Distance(target.position, transform.position));
        Debug.Log(distanceMod);
    }
}