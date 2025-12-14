using Internal.Scripts.Road.Graph;
using UnityEngine;

namespace Internal.Scripts.Road.Renderer
{
    public interface IRoadGraphRenderer
    {
        void Build(RoadGraph graph, GameObject owner);
    }
}
