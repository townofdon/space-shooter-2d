
using UnityEngine;

namespace UI {

    public class BackgroundScroller : MonoBehaviour {

        [SerializeField] Vector2 scrollSpeed = new Vector2(0f, 0.5f);

        // cached
        SpriteRenderer sr;
        Material mat;

        // state
        Vector2 scroll = Vector2.zero;
        Vector2 scrollDelta = Vector2.zero;

        public void RestoreScroll() {
            scrollDelta = scrollSpeed;
        }

        public Vector2 GetScrollDelta() {
            return scrollDelta;
        }

        public void SetScrollDelta(Vector2 value) {
            scrollDelta = value;
        }

        void Start() {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null) mat = sr.material;
            if (mat != null) scroll = mat.mainTextureOffset;
            scrollDelta = scrollSpeed;
        }

        void Update() {
            scroll.x += scrollDelta.x * Time.deltaTime;
            scroll.y += scrollDelta.y * Time.deltaTime;
            mat.mainTextureOffset = scroll;
        }
    }
}
