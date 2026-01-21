using System;
using Internal.Scripts.InteractableObjects;
using Internal.Scripts.Road.Nodes.UI;
using Internal.Scripts.Road.Nodes.UI.CollidersViewer;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.Player.UI.StartMovement
{
    public class PlayerStartMovement : IPlayerStartMovement, IInitializable, IDisposable
    {
        public event Action<string> OnChooseNode;

        private readonly Button _startMovementButton;
        private readonly INodesCollidersViewer _nodesCollidersViewer;

        private string _currentPlayerNode;

        public PlayerStartMovement(Button startMovementButton, INodesCollidersViewer nodesCollidersViewer)
        {
            _startMovementButton = startMovementButton;
            _nodesCollidersViewer = nodesCollidersViewer;
        }
        
        public void Initialize()
        {
            _startMovementButton.onClick.AddListener(ShowColliders);
        }

        public void Dispose()
        {
            _startMovementButton.onClick.RemoveAllListeners();
        }

        public void SetCurrentPlayerNode(string node)
        {
            _currentPlayerNode = node;
        }

        public void FinishPath()
        {
            _startMovementButton.gameObject.SetActive(true);
        }
        
        private void ShowColliders()
        {
            _startMovementButton.gameObject.SetActive(false);
            _nodesCollidersViewer.ShowColliders();
            foreach (NodeView nodeView in _nodesCollidersViewer.GetAllNodes())
            {
                if (nodeView.NodeId == _currentPlayerNode)
                {
                    nodeView.gameObject.SetActive(false);
                    continue;
                }
                nodeView.OnClick += OnChooseNodeCollider;
            }
        }

        private void OnChooseNodeCollider(IInteractableObject interactableObject)
        {
            if (interactableObject is not NodeView view) return;
            
            _nodesCollidersViewer.HideColliders();
            OnChooseNode?.Invoke(view.NodeId);
            foreach (NodeView nodeView in _nodesCollidersViewer.GetAllNodes())
            {
                if (nodeView.NodeId == _currentPlayerNode) continue;
                nodeView.OnClick -= OnChooseNodeCollider;
            }
        }
    }
}