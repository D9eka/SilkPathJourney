using UnityEngine;

namespace Internal.Scripts.UI.Arrow.PositionCalculation
{
    public class GroundSnapper
    {
        private const float RAYCAST_UP_OFFSET = 500f;
        private const float RAYCAST_MAX_DISTANCE = 1000f;
        
        private readonly LayerMask _groundLayerMask;

        public GroundSnapper(LayerMask groundLayerMask)
        {
            _groundLayerMask = groundLayerMask;
        }
        
        public float GetGroundHeightAtXZ(Vector3 xzPosition)
        {
            Vector3 rayOrigin = new Vector3(xzPosition.x, RAYCAST_UP_OFFSET, xzPosition.z);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, RAYCAST_MAX_DISTANCE, _groundLayerMask))
            {
                //Debug.Log($"[GroundSnapper] HIT (masked) Y={hit.point.y:F1} obj={hit.collider.name} layer={hit.collider.gameObject.layer}");
                return hit.point.y;
            }

            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, RAYCAST_MAX_DISTANCE))
            {
                Debug.LogWarning($"[GroundSnapper] MISS masked (mask={_groundLayerMask.value}), but FALLBACK HIT: Y={hit.point.y:F1} obj={hit.collider.name} layer={LayerMask.LayerToName(hit.collider.gameObject.layer)}({hit.collider.gameObject.layer})");
                return hit.point.y;
            }

            Debug.LogWarning($"[GroundSnapper] TOTAL MISS at XZ=({xzPosition.x:F1}, {xzPosition.z:F1})");
            return xzPosition.y;
        }
        
        public Vector3 SnapToGround(Vector3 position, float heightAboveGround = 0.5f)
        {
            float groundY = GetGroundHeightAtXZ(position);
            return new Vector3(position.x, groundY + heightAboveGround, position.z);
        }
    }
}
