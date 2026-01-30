using UnityEngine;

namespace Internal.Scripts.UI.Arrow.PositionCalculation
{
    public class GroundSnapper
    {
        private const float RAYCAST_UP_OFFSET = 100f;
        private const float RAYCAST_MAX_DISTANCE = 200f;
        
        private readonly LayerMask _groundLayerMask;

        public GroundSnapper(LayerMask groundLayerMask)
        {
            _groundLayerMask = groundLayerMask;
        }
        
        public float GetGroundHeightAtXZ(Vector3 xzPosition)
        {
            Vector3 rayOrigin = xzPosition + Vector3.up * RAYCAST_UP_OFFSET;
            Vector3 rayDirection = Vector3.down;

            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, RAYCAST_MAX_DISTANCE, _groundLayerMask))
            {
                return hit.point.y;
            }
            return xzPosition.y;
        }
        
        public Vector3 SnapToGround(Vector3 position, float heightAboveGround = 0.5f)
        {
            float groundY = GetGroundHeightAtXZ(position);
            return new Vector3(position.x, groundY + heightAboveGround, position.z);
        }
    }
}
