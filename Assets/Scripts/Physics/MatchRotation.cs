using UnityEngine;

namespace Physics {

    public class MatchRotation : MonoBehaviour {
        [SerializeField] Transform target;

        Quaternion rotation;

        void Update() {
            if (target == null) return;

            rotation = Quaternion.Euler(0f, 0f, target.rotation.eulerAngles.z);

            // this wasn't working to rotate the sub particle system - my hunch is that
            // the parent particle system automagically updates the transform of the child
            transform.rotation = rotation;
        }
    }
}
