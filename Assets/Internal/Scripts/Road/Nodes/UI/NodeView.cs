using Internal.Scripts.InteractableObjects;

namespace Internal.Scripts.Road.Nodes.UI
{
    public class NodeView : InteractableObjectView
    {
        public string NodeId { get; private set; }

        public void Initialize(string nodeId)
        {
            NodeId = nodeId;
        }
    }
}