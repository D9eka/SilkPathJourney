using UnityEngine;

namespace Internal.Scripts.Camera
{
    public class CameraBounds
    {
        private readonly Bounds _strategicBounds;
        private readonly UnityEngine.Camera _camera;
        private Bounds? _overrideBounds;

        public Vector2 Center { get; }

        public CameraBounds(Collider boundsCollider, Transform center, UnityEngine.Camera camera)
        {
            _strategicBounds = boundsCollider.bounds;
            _camera = camera;
            Center = new Vector2(center.position.x, center.position.z);
        }

        public void SetOverrideBounds(Vector2 center, Vector2 size)
        {
            _overrideBounds = new Bounds(
                new Vector3(center.x, 0f, center.y),
                new Vector3(size.x, 100f, size.y));
        }

        public void ClearOverrideBounds()
        {
            _overrideBounds = null;
        }

        public Vector2 Clamp(Vector2 worldTarget)
        {
            Bounds active = _overrideBounds ?? _strategicBounds;
            Vector2 half = CalculateViewportHalfExtent();

            float minX = active.min.x + half.x;
            float maxX = active.max.x - half.x;
            float minZ = active.min.z + half.y;
            float maxZ = active.max.z - half.y;

            float cx = minX > maxX ? (active.min.x + active.max.x) * 0.5f : Mathf.Clamp(worldTarget.x, minX, maxX);
            float cz = minZ > maxZ ? (active.min.z + active.max.z) * 0.5f : Mathf.Clamp(worldTarget.y, minZ, maxZ);

            return new Vector2(cx, cz);
        }

        private Vector2 CalculateViewportHalfExtent()
        {
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            Vector3 c = ViewportToGround(ground, new Vector3(0.5f, 0.5f));
            Vector3 bl = ViewportToGround(ground, new Vector3(0, 0));
            Vector3 br = ViewportToGround(ground, new Vector3(1, 0));
            Vector3 tl = ViewportToGround(ground, new Vector3(0, 1));
            Vector3 tr = ViewportToGround(ground, new Vector3(1, 1));

            float maxDx = Mathf.Max(
                Mathf.Max(Mathf.Abs(bl.x - c.x), Mathf.Abs(br.x - c.x)),
                Mathf.Max(Mathf.Abs(tl.x - c.x), Mathf.Abs(tr.x - c.x)));
            float maxDz = Mathf.Max(
                Mathf.Max(Mathf.Abs(bl.z - c.z), Mathf.Abs(br.z - c.z)),
                Mathf.Max(Mathf.Abs(tl.z - c.z), Mathf.Abs(tr.z - c.z)));

            return new Vector2(maxDx, maxDz);
        }

        private Vector3 ViewportToGround(Plane plane, Vector3 viewportPoint)
        {
            Ray ray = _camera.ViewportPointToRay(viewportPoint);
            if (plane.Raycast(ray, out float dist) && dist < 500f)
                return ray.GetPoint(dist);
            return ray.GetPoint(500f);
        }
    }
}
