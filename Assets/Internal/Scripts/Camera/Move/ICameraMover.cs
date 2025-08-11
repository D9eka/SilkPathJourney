namespace Internal.Scripts.Camera.Move
{
    public interface ICameraMover
    {
        void MoveTo(UnityEngine.Vector2 position, System.Action onComplete = null);
        void MoveTo(UnityEngine.Vector2 position, float duration, System.Action onComplete = null);
    }
}