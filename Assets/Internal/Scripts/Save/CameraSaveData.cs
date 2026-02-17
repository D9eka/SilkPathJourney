using System;

namespace Internal.Scripts.Save
{
    [Serializable]
    public class CameraSaveData
    {
        public float WorldTargetX;
        public float WorldTargetZ;
        public float ZoomSize;
        public string ActiveDetailScene;
    }
}
