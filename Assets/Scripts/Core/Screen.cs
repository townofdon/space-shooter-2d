
using UnityEngine;

namespace Core {

    public class ScreenUtils
    {
        public static bool IsObjectOnScreen(GameObject obj, Camera camera = null, float offscreenPadding = 1f)
        {
            if (camera == null) camera = Camera.main;
            Vector2 _minBoundsWorld = camera.ViewportToWorldPoint(Vector2.zero) - (Vector3)Vector2.one * offscreenPadding;
            Vector2 _maxBoundsWorld = camera.ViewportToWorldPoint(Vector2.one) + (Vector3)Vector2.one * offscreenPadding;
            return
                obj.transform.position.x > _minBoundsWorld.x &&
                obj.transform.position.x < _maxBoundsWorld.x &&
                obj.transform.position.y > _minBoundsWorld.y &&
                obj.transform.position.y < _maxBoundsWorld.y;
        }

        public static Vector2 GetScreenSize(Camera camera, float padding = 0f) {
            return camera.ViewportToWorldPoint(new Vector2(1f, 1f)) - camera.ScreenToWorldPoint(Vector3.one * padding);
        }
    }
}

