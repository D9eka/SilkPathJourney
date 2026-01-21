using System.Collections.Generic;

namespace Internal.Scripts.Road.Nodes.UI.CollidersViewer
{
    public interface INodesCollidersViewer
    {
        List<NodeView> GetAllNodes();
        
        void ShowColliders();
        void HideColliders();
    }
}