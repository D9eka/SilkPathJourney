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

        public void FinishPath()
        {
            _startMovementButton.gameObject.SetActive(true);
        }
        
        private void ShowColliders()
        {
            _startMovementButton.gameObject.SetActive(false);
            _nodesCollidersViewer.ShowColliders();
            foreach (NodeView nodesCollider in _nodesCollidersViewer.GetAllNodes())
            {
                nodesCollider.OnClick += OnChooseNodeCollider;
            }
        }

        private void OnChooseNodeCollider(IInteractableObject interactableObject)
        {
            if (interactableObject is not NodeView nodeCollider) return;
            
            _nodesCollidersViewer.HideColliders();
            OnChooseNode?.Invoke(nodeCollider.NodeId);
            foreach (NodeView nodesCollider in _nodesCollidersViewer.GetAllNodes())
            {
                nodesCollider.OnClick -= OnChooseNodeCollider;
            }
        }
    }
}