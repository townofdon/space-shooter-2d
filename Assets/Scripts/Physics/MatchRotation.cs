using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Physics {

    public class MatchRotation : MonoBehaviour {
        [SerializeField] Transform target;

        void Update() {
            if (target == null) return;
            transform.rotation = target.rotation;
        }
    }
}
