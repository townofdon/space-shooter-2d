using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    
    public class OffscreenMarker : MonoBehaviour
    {
        [SerializeField] Image marker;
        [SerializeField] Transform target;
        [SerializeField] Color enemyFlag;
        [SerializeField] Color dangerFlag;
        [SerializeField] Color infoFlag;
        [SerializeField] Color friendlyFlag;

        Vector2 minBounds;
        Vector2 maxBounds;
        Vector2 minBoundsWorld;
        Vector2 maxBoundsWorld;
        Vector2 screenSize;
        Vector2 rotateTowards;
        float aspectRatio;
        float offscreenDistanceBeforeShowingMarker = 0.5f;
        float padding = 15f;
        Vector3 markerPosition;
        Vector3 markerPositionWorld;
        Coroutine removeFiringFlag;

        public void FlagEnemy() {
            marker.color = enemyFlag;
        }
        public void FlagEnemyFiring() {
            if (removeFiringFlag != null) StopCoroutine(removeFiringFlag);
            marker.color = dangerFlag;
            removeFiringFlag = StartCoroutine(IRemoveFiringFlag());
        }
        public void FlagIncoming() {
            marker.color = dangerFlag;
        }
        public void FlagFriendly() {
            marker.color = friendlyFlag;
        }

        IEnumerator IRemoveFiringFlag() {
            yield return new WaitForSeconds(2f);
            marker.color = enemyFlag;
        }

        void Start() {
            minBounds = Camera.main.ViewportToScreenPoint(new Vector2(0f, 0f)) + Vector3.one * padding;
            maxBounds = Camera.main.ViewportToScreenPoint(new Vector2(1f, 1f)) - Vector3.one * padding;
            minBoundsWorld = Camera.main.ViewportToWorldPoint(new Vector2(0f, 0f)) - (Vector3)Vector2.one * offscreenDistanceBeforeShowingMarker;
            maxBoundsWorld = Camera.main.ViewportToWorldPoint(new Vector2(1f, 1f)) + (Vector3)Vector2.one * offscreenDistanceBeforeShowingMarker;
            screenSize = Camera.main.ViewportToWorldPoint(new Vector2(1f, 1f)) - Camera.main.ScreenToWorldPoint(Vector3.one * padding);
            aspectRatio = Camera.main.aspect;
            markerPosition = marker.rectTransform.position;
            marker.enabled = false;
            marker.color = enemyFlag;

            Debug.Log(minBoundsWorld);
            Debug.Log(maxBoundsWorld);
        }

        void Update() {
            if (target == null || target.gameObject == null || !target.gameObject.activeSelf) {
                marker.enabled = false;
                return;
            }
            if (IsTrackedOutsideScreen()) {
                marker.enabled = true;
                FollowPythagoras();
            } else {
                marker.enabled = false;
            }
        }

        bool IsTrackedOutsideScreen() {
            return
                target.position.x < minBoundsWorld.x ||
                target.position.y < minBoundsWorld.y ||
                target.position.x > maxBoundsWorld.x ||
                target.position.y > maxBoundsWorld.y;
        }

        // so turns out you actually don't need the Pythagoran theorum for this, however the name is still cool and all so I'll keep it
        void FollowPythagoras() {
            if (marker == null) return;
            Vector2 dirX = target.position.x >= 0f ? Vector2.right : Vector2.left;
            Vector2 dirY = target.position.y >= 0f ? Vector2.up : Vector2.down;
            float angleFromX = Vector2.Angle(dirX, target.position.normalized) * aspectRatio;
            float angleFromY = Vector2.Angle(dirY, target.position.normalized) / aspectRatio;
            // work with the smallest of two angles, taking into account the screen's aspect ratio
            if (angleFromX <= angleFromY) {
                rotateTowards = dirX;
                // calculate to SCREEN_WIDTH / 2, signed by dirX
                float a = (screenSize.x / 2f) * dirX.x;
                // calculate y offset using proportional triangle theorum (I just made this up but it's prob a trigonometry theorum for real) - see notes
                float b = (a * target.position.y) / target.position.x;
                markerPositionWorld = new Vector2(a, b);
            } else {
                rotateTowards = dirY;
                // calculate to SCREEN_HEIGHT / 2, signed by dirY
                float a = (screenSize.y / 2f) * dirY.y;
                // proportional triangle
                float b = (a * target.position.x) / target.position.y;
                markerPositionWorld = new Vector2(b, a);
            }
            markerPosition = Camera.main.WorldToScreenPoint(markerPositionWorld);
            marker.rectTransform.position = markerPosition;
            marker.rectTransform.rotation = GetMarkerRotation();
        }

        Quaternion GetMarkerRotation() {
            return Quaternion.FromToRotation(Vector2.up, rotateTowards);
        }
    }
}
