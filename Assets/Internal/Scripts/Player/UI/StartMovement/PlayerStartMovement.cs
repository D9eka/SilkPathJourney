using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Internal.Scripts.InteractableObjects;
using Internal.Scripts.Road.Nodes.UI;
using Internal.Scripts.Road.Nodes.UI.NodesViewer;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.Player.UI.StartMovement
{
    public class PlayerStartMovement : IPlayerStartMovement, IInitializable, IDisposable
    {
        public event Action<string> OnChooseNode;

        private readonly INodesViewer _nodesViewer;
        private readonly Button _startTargetSelectionButton;
        private readonly Button _cancelTargetSelectionButton;

        private string _currentPlayerNode;
        private UniTaskCompletionSource<string> _tcs;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isChoosingTarget;

        public bool IsChoosingTarget => _isChoosingTarget;

        public PlayerStartMovement(INodesViewer nodesViewer, 
            Button startTargetSelectionButton, Button cancelTargetSelectionButton)
        {
            _nodesViewer = nodesViewer;
            _startTargetSelectionButton = startTargetSelectionButton;
            _cancelTargetSelectionButton = cancelTargetSelectionButton;
        }

        public void Initialize()
        {
            _startTargetSelectionButton.onClick.AddListener(StartSelection);
            
            _cancelTargetSelectionButton.gameObject.SetActive(false);
            _cancelTargetSelectionButton.onClick.AddListener(CancelSelection);
        }

        public void Dispose()
        {
            _startTargetSelectionButton.onClick.RemoveAllListeners();
        }

        public void SetCurrentPlayerNode(string node)
        {
            _currentPlayerNode = node;
        }

        public void FinishPath()
        {
            _startTargetSelectionButton.gameObject.SetActive(true);
        }
        
        private async void StartSelection()
        {
            _isChoosingTarget = true;
            _cancellationTokenSource = new CancellationTokenSource();
            _tcs = new UniTaskCompletionSource<string>();
            _cancellationTokenSource.Token.Register(() => 
            {
                _tcs.TrySetCanceled();
            });
            
            _startTargetSelectionButton.gameObject.SetActive(false);
            _cancelTargetSelectionButton.gameObject.SetActive(true);
            _nodesViewer.ShowNodes();
            SubscribeToNodes();

            try
            {
                OnChooseNode?.Invoke(await _tcs.Task);
            }
            catch (OperationCanceledException)
            {
                FinishPath();
            }
            finally
            {
                _nodesViewer.HideNodes();
                UnsubscribeToNodes();
                _cancelTargetSelectionButton.gameObject.SetActive(false);
                _tcs = null;
                _isChoosingTarget = false;
            }
        }
        
        private void CancelSelection()
        {
            _tcs?.TrySetCanceled();
        }

        private void SubscribeToNodes()
        {
            foreach (NodeView nodeView in _nodesViewer.GetAllNodes())
            {
                if (nodeView.NodeId == _currentPlayerNode)
                {
                    nodeView.gameObject.SetActive(false);
                    continue;
                }
                nodeView.OnClick += OnChooseNodeCollider;
            }
        }
        
        private void UnsubscribeToNodes()
        {
            foreach (NodeView nodeView in _nodesViewer.GetAllNodes())
            {
                if (nodeView.NodeId == _currentPlayerNode) continue;
                nodeView.OnClick -= OnChooseNodeCollider;
            }
        }

        private void OnChooseNodeCollider(IInteractableObject interactableObject)
        {
            if (interactableObject is not NodeView view) return;
            
            _tcs.TrySetResult(view.NodeId);
        }
    }
}
