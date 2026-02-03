using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Internal.Scripts.InteractableObjects;
using Internal.Scripts.Road.Nodes.UI;
using Internal.Scripts.Road.Nodes.UI.NodesViewer;

namespace Internal.Scripts.Player.StartMovement
{
    public class PlayerStartMovement : IPlayerStartMovement, IDisposable
    {
        public event Action<string> OnChooseNode;
        public event Action<bool> OnSelectionStateChanged;

        private readonly INodesViewer _nodesViewer;

        private string _currentPlayerNode;
        private UniTaskCompletionSource<string> _tcs;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isChoosingTarget;

        public bool IsChoosingTarget => _isChoosingTarget;

        public PlayerStartMovement(INodesViewer nodesViewer)
        {
            _nodesViewer = nodesViewer;
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }

        public void SetCurrentPlayerNode(string node)
        {
            _currentPlayerNode = node;
        }

        public void BeginSelection()
        {
            if (_isChoosingTarget)
                return;

            StartSelection();
        }

        private async void StartSelection()
        {
            SetSelectionState(true);
            _cancellationTokenSource = new CancellationTokenSource();
            _tcs = new UniTaskCompletionSource<string>();
            _cancellationTokenSource.Token.Register(() => 
            {
                _tcs.TrySetCanceled();
            });
            _nodesViewer.ShowNodes();
            SubscribeToNodes();

            try
            {
                OnChooseNode?.Invoke(await _tcs.Task);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _nodesViewer.HideNodes();
                UnsubscribeToNodes();
                _tcs = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                SetSelectionState(false);
            }
        }
        
        public void CancelSelection()
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

        private void SetSelectionState(bool state)
        {
            if (_isChoosingTarget == state)
                return;

            _isChoosingTarget = state;
            OnSelectionStateChanged?.Invoke(state);
        }
    }
}
