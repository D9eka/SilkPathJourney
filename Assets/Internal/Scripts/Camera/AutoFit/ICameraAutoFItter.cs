using System;

namespace Internal.Scripts.Camera.AutoFit
{
    public interface ICameraAutoFitter
    {
        void FocusOnObjects(UnityEngine.Transform[] targets, Action onComplete);
    }
}