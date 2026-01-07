using UnityEngine;

namespace Internal.Scripts.Road.Path
{
    public readonly struct RoadPose
    {
        public Vector3 Position { get; }
        public Vector3 Forward { get; }

        public RoadPose(Vector3 position, Vector3 forward)
        {
            Position = position;
            Forward = forward;
        }
    }
}
