using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI {

    public class BackgroundScroller : MonoBehaviour {

        [SerializeField] Vector2 scrollSpeed = new Vector2(0f, 0.5f);

        // cached
        SpriteRenderer sr;
        Material mat;

        // state
        Vector2 scroll = Vector2.zero;

        void Start() {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null) mat = sr.material;
            if (mat != null) scroll = mat.mainTextureOffset;
        }

        void Update() {
            scroll.x += scrollSpeed.x * Time.deltaTime;
            scroll.y += scrollSpeed.y * Time.deltaTime;
            mat.mainTextureOffset = scroll;
        }
    }
}
