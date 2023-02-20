using UnityEngine;
using UnityEngine.UI;

using Core;

namespace UI
{

    public enum FlagType {
        Enemy,
        Incoming,
        Friendly,
        Info,
    }
    
    public class OffscreenMarker : MonoBehaviour
    {
        [SerializeField] Image marker;
        [SerializeField] Transform target;
        [SerializeField] FlagType flag;
        [SerializeField] Color enemyFlag;
        [SerializeField] Color dangerFlag;
        [SerializeField] Color infoFlag;
        [SerializeField] Color friendlyFlag;
        [SerializeField] float outOfRange = 20f;
        [SerializeField] bool debug = false;

        [Space]

        [SerializeField] bool showNorth = true;
        [SerializeField] bool showSouth = true;
        [SerializeField] bool showEast = true;
        [SerializeField] bool showWest = true;

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
        Canvas canvas;

        public void SetOrdinals(bool north, bool east, bool south, bool west) {
            showNorth = north;
            showEast = east;
            showSouth = south;
            showWest = west;
        }
        public void SetTarget(Transform _target) {
            target = _target;
        }
        public void SetFlagType(FlagType type) {
            flag = type;
            InitFlag();
        }
        public void Disable() {
            gameObject.SetActive(false);
            marker.enabled = false;
            target = null;
        }

        void Start() {
            minBounds = Utils.GetCamera().ViewportToScreenPoint(new Vector2(0f, 0f)) + Vector3.one * padding;
            maxBounds = Utils.GetCamera().ViewportToScreenPoint(new Vector2(1f, 1f)) - Vector3.one * padding;
            minBoundsWorld = Utils.GetCamera().ViewportToWorldPoint(new Vector2(0f, 0f)) - (Vector3)Vector2.one * offscreenDistanceBeforeShowingMarker;
            maxBoundsWorld = Utils.GetCamera().ViewportToWorldPoint(new Vector2(1f, 1f)) + (Vector3)Vector2.one * offscreenDistanceBeforeShowingMarker;
            screenSize = Utils.GetCamera().ViewportToWorldPoint(new Vector2(1f, 1f)) - Utils.GetCamera().ScreenToWorldPoint(Vector3.one * padding);
            aspectRatio = Utils.GetCamera().aspect;
            markerPosition = marker.rectTransform.position;
            marker.enabled = false;
            canvas = marker.GetComponentInParent<Canvas>();
            canvas.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            marker.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            InitFlag();
        }

        void Update() {
            if (target == null || target.gameObject == null || !target.gameObject.activeSelf) {
                marker.enabled = false;
                return;
            }
            if (IsTrackedOutOfRange()) {
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

        void InitFlag() {
            switch (flag) {
                case FlagType.Incoming:
                    marker.color = dangerFlag;
                    break;
                case FlagType.Friendly:
                    marker.color = friendlyFlag;
                    break;
                case FlagType.Info:
                    marker.color = infoFlag;
                    break;
                case FlagType.Enemy:
                default:
                    marker.color = enemyFlag;
                    break;
            }
        }

        bool IsTrackedOutOfRange() {
            return IsTrackedOutsideScreen(outOfRange);
        }

        bool IsTrackedOutsideScreen(float mod = 0f) {
            return
                (showWest && target.position.x < minBoundsWorld.x - mod) ||
                (showSouth && target.position.y < minBoundsWorld.y - mod) ||
                (showEast && target.position.x > maxBoundsWorld.x + mod) ||
                (showNorth && target.position.y > maxBoundsWorld.y + mod);
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
            markerPosition = Utils.GetCamera().WorldToScreenPoint(markerPositionWorld + Utils.GetCameraPosition());
            marker.rectTransform.position = markerPosition;
            marker.rectTransform.rotation = GetMarkerRotation();
        }

        Quaternion GetMarkerRotation() {
            return Quaternion.FromToRotation(Vector2.up, rotateTowards);
        }

        void OnGUI()
        {
            if (!debug) return;
            if (GUILayout.Button("Flag Enemy")) {
                SetFlagType(FlagType.Enemy);
            }
            if (GUILayout.Button("Flag Incoming")) {
                SetFlagType(FlagType.Incoming);
            }
            if (GUILayout.Button("Flag Info")) {
                SetFlagType(FlagType.Info);
            }
            if (GUILayout.Button("Flag Friendly")) {
                SetFlagType(FlagType.Friendly);
            }
        }
    }
}
