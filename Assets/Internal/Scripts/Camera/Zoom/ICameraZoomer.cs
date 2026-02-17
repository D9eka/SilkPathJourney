namespace Internal.Scripts.Camera.Zoom
{
    public interface ICameraZoomer
    {
        public float Size { get; }

        void ZoomTo(float size, System.Action onComplete = null);
        void ZoomTo(float size, UnityEngine.Vector3? targetWorldPos, System.Action onComplete = null);
        void ZoomTo(float size, float duration, System.Action onComplete = null);
        void ZoomTo(float size, UnityEngine.Vector3? targetWorldPos, float duration, System.Action onComplete = null);
    }
}