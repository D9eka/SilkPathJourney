using System;

namespace Internal.Scripts.Import.Editor.Roads.DTO
{
    [Serializable]
    public class NodeJson
    {
        public string NodeId;
        public string Biome;
    }

    [Serializable]
    public class RoadJsonCombined
    {
        public int Version;
        public string RelativeTo;
        public string Space;
        public RoadJsonSingle[] Roads;
        public NodeJson[] Nodes;
    }
}
