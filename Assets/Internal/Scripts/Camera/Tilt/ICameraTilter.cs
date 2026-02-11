using System;

namespace Internal.Scripts.Camera.Tilt
{
    public interface ICameraTilter
    {
        float CurrentAngle { get; }
        bool IsAnimating { get; }
        void TiltTo(float angle, float duration, Action onComplete = null);
    }
}
