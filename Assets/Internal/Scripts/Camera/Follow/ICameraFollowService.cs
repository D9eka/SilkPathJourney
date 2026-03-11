using System;

namespace Internal.Scripts.Camera.Follow
{
    public interface ICameraFollowService
    {
        bool IsFollowing { get; }
        event Action<bool> OnFollowStateChanged;
        void StartFollowing();
        void StopFollowing();
    }
}
