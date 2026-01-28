using System;

namespace Internal.Scripts.World.Editor.RoadImporter.DTO
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