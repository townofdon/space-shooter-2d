using UnityEngine;

namespace Core {

    public class Utils
    {
        public static void Elapse(ref float timer, float amount, float max = Mathf.Infinity)
        {
            timer = Mathf.Min(timer + amount, max + 1f);
        }

        public static Vector3 FlipX(Vector3 v) {
            return Vector3.Reflect(v, Vector3.left);
            // v.x *= -1;
            // return v;
        }

        public static void DebugDrawRect(Vector3 pos, float size, Color color)
        {
            Debug.DrawLine(new Vector3(pos.x - size / 2, pos.y + size / 2, 0f), new Vector3(pos.x + size / 2, pos.y + size / 2, 0f), color);
            Debug.DrawLine(new Vector3(pos.x - size / 2, pos.y + size / 2, 0f), new Vector3(pos.x - size / 2, pos.y - size / 2, 0f), color);
            Debug.DrawLine(new Vector3(pos.x - size / 2, pos.y - size / 2, 0f), new Vector3(pos.x + size / 2, pos.y - size / 2, 0f), color);
            Debug.DrawLine(new Vector3(pos.x + size / 2, pos.y + size / 2, 0f), new Vector3(pos.x + size / 2, pos.y - size / 2, 0f), color);
        }
        public static void DebugDrawRect(Vector3 position, float size)
        {
            DebugDrawRect(position, size, Color.red);
        }
        public static void DebugDrawRect(Vector3 position, Color color)
        {
            DebugDrawRect(position, .1f, color);
        }
        public static void DebugDrawRect(Vector3 position)
        {
            DebugDrawRect(position, .1f, Color.red);
        }

        // check to see whether a LayerMask contains a layer
        // see: https://answers.unity.com/questions/50279/check-if-layer-is-in-layermask.html
        public static bool LayerMaskContainsLayer(int mask, int layer) {
            bool contains = ((mask & (1 << layer)) != 0);
            return contains;
        }

        // get the layer num from a layermask
        // see: https://forum.unity.com/threads/get-the-layernumber-from-a-layermask.114553/#post-3021162
        public static int ToLayer(int layerMask) {
            int result = layerMask > 0 ? 0 : 31;
            while( layerMask > 1 ) {
                layerMask = layerMask >> 1;
                result++;
            }
            return result;
        }

        
        public static bool shouldBlink(float timeElapsed, float rate) {
            return (timeElapsed / rate % 2f) < 1f;
        }

        // Get a child game object by name or tag
        // see: https://answers.unity.com/questions/183649/how-to-find-a-child-gameobject-by-name.html
        public static GameObject FindChild(GameObject parent, string lookup) {
            Transform[] ts = parent.transform.GetComponentsInChildren<Transform>();
            foreach (Transform t in ts) if (t.gameObject.name == lookup || t.gameObject.tag == lookup) return t.gameObject;
            return null;
        }

        // Helper method that gets a component and also asserts its presence - useful for early-error pattern
        // 
        // USAGE:
        // ```
        // Rigidbody2D rb = GetRequiredComponent<Rigiidbody2D>(this);
        // ```
        public static T GetRequiredComponent<T>(GameObject gameObject)
        {
            AppIntegrity.AssertPresent<GameObject>(gameObject);
            T component = gameObject.GetComponent<T>();
            AppIntegrity.AssertPresent<T>(component);
            return component;
        }

        public static GameObject GetRequiredChild(GameObject parent, string lookup)
        {
            GameObject child = Utils.FindChild(parent, lookup);
            AppIntegrity.AssertPresent<GameObject>(child);
            return child;
        }

        public static string ToTimeString(float t) {
            string minutes = Mathf.Floor(t / 60).ToString("0");
            string seconds = (t % 60).ToString("00");
            return string.Format("{0}:{1}", minutes, seconds);
        }

        public static Vector2 RandomVector2(float magnitude = 1f) {
            return new Vector2(UnityEngine.Random.Range(0f, magnitude), UnityEngine.Random.Range(0f, magnitude)).normalized;
        }

        public static float RandomVariance(float initialValue, float variance = 0f, float min = float.MinValue, float max = float.MaxValue)
        {
            if (variance <= 0) return initialValue;
            return Mathf.Clamp(initialValue * (1 + UnityEngine.Random.Range(-variance / 2f, variance / 2f)), min, max);
        }

        public static bool RandomBool(float threshold = 0.5f) {
            return Random.value > threshold;
        }

        public static bool HasTag(GameObject gameObject, string tag) {
            Transform current = gameObject.transform;

            while (current != null) {
                if (current.tag == tag) return true;
                current = current.transform.parent;
            }

            return false;
        }

        public static Vector2 GetNearestCardinal(Vector2 direction) {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
                return direction.x >= 0f ? Vector2.right : Vector2.left;
            } else {
                return direction.y >= 0f ? Vector2.up : Vector2.down;
            }
        }

        public static T ManageSingleton<T>(T instance = null, T incoming = null) where T : UnityEngine.MonoBehaviour {
            var objectsInScene = Object.FindObjectsOfType<T>();
            if (objectsInScene.Length > 1 && instance != incoming) {
                Object.Destroy(incoming.gameObject);
                return instance;
            } else {
                Object.DontDestroyOnLoad(incoming.gameObject);
                return incoming;
            }
        }

        public static void CleanupSingleton<T>(T instance = null) where T : UnityEngine.MonoBehaviour {
            if (instance != null) {
                Object.Destroy(instance.gameObject);
            }
            GameObject[] items = Object.FindObjectsOfType<T>() as GameObject[];
            foreach (var item in items)
            {
                if (item != null && item.gameObject != null) {
                    Object.Destroy(item.gameObject);
                }
            }

        }

        // cached state
        static bool hasInitializedBounds = false;
        static Camera cachedCamera;
        static Vector2 minScreenBoundsWorld;
        static Vector2 maxScreenBoundsWorld;

        public static Camera GetCamera(Camera camera = null) {
            if (camera != null) return camera;
            if (cachedCamera == null) cachedCamera = Camera.main;
            return cachedCamera;
        }

        public static (Vector2, Vector2) GetScreenBounds(Camera camera = null, float offscreenPadding = 1f, bool forceCalc = false) {
            if (hasInitializedBounds && !forceCalc) return (
                minScreenBoundsWorld - Vector2.one * offscreenPadding,
                maxScreenBoundsWorld + Vector2.one * offscreenPadding);
            camera = GetCamera(camera);
            minScreenBoundsWorld = camera.ViewportToWorldPoint(Vector2.zero);
            maxScreenBoundsWorld = camera.ViewportToWorldPoint(Vector2.one);
            hasInitializedBounds = true;
            return (
                minScreenBoundsWorld - Vector2.one * offscreenPadding,
                maxScreenBoundsWorld + Vector2.one * offscreenPadding);
        }

        public static bool IsObjectOnScreen(GameObject obj, Camera camera = null, float offscreenPadding = 1f) {
            camera = GetCamera(camera);
            (Vector2 _minBoundsWorld, Vector2 _maxBoundsWorld) = Utils.GetScreenBounds(camera, offscreenPadding);
            return
                obj.transform.position.x > _minBoundsWorld.x &&
                obj.transform.position.x < _maxBoundsWorld.x &&
                obj.transform.position.y > _minBoundsWorld.y &&
                obj.transform.position.y < _maxBoundsWorld.y;
        }

        public static bool IsObjectHeadingAwayFromCenterScreen(GameObject obj, Rigidbody2D rb, Camera camera = null) {
            camera = GetCamera(camera);
            if (obj.transform.position.x > camera.transform.position.x && rb.velocity.x > 0) return true;
            if (obj.transform.position.x < camera.transform.position.x && rb.velocity.x < 0) return true;
            if (obj.transform.position.y > camera.transform.position.y && rb.velocity.y > 0) return true;
            if (obj.transform.position.y < camera.transform.position.y && rb.velocity.y < 0) return true;
            return false;
        }

        public static Vector2 GetScreenSize(Camera camera, float padding = 0f) {
            return camera.ViewportToWorldPoint(new Vector2(1f, 1f)) - camera.ScreenToWorldPoint(Vector3.one * padding);
        }

        public static float AbsAngle(float signedAngle) {
            return signedAngle >= 0f ? signedAngle % 360f : 360f + signedAngle % 360f;
        }

        public static int GetRootInstanceId(GameObject gameObject) {
            Transform current = gameObject.transform;

            while (current.transform.parent != null && current.transform.parent.tag == gameObject.tag) {
                current = current.transform.parent;
            }

            return current.gameObject.GetInstanceID();
        }

        public static void __NOOP__(float num, Damage.DamageType damageType, bool isDamageByPlayer) { }
        public static void __NOOP__(float num) {}
        public static void __NOOP__() {}
    }
}

