using System.Collections.Generic;

namespace Internal.Scripts.Road.Nodes.UI.NodesViewer
{
    public interface INodesViewer
    {
        List<NodeView> GetAllNodes();
        
        void ShowNodes();
        void HideNodes();
    }
}