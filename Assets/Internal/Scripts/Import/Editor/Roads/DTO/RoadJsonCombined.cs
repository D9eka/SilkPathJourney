using System;

namespace Internal.Scripts.Import.Editor.Roads.DTO
{
    [Serializable]
    public class RoadJsonCombined
    {
        public int Version;
        public string RelativeTo;
        public string Space;
        public RoadJsonSingle[] Roads;
    }
}
