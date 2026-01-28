using System;

namespace Internal.Scripts.World.Editor.RoadImporter.DTO
{
    [Serializable]
    public class RoadJsonSingle
    {
        public int Version;
        public string RoadId;
        public string RelativeTo;
        public string Space;
        public Meta Meta;
        public Point[] PointsLocal;
        public Endpoints Endpoints;
    }
}