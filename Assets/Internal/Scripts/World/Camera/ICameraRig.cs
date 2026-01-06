
namespace Internal.Scripts.World.Camera
{
    public interface ICameraRig
    {
        public float Size { get; }

        public void ChangeSize(float sizeDelta);
    }
}