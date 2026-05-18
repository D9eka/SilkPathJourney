using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.UI;
using Internal.Scripts.InteractableObjects;
using UnityEngine;

namespace Internal.Scripts.Player.StartMovement
{
    public class PlayerStartMovement : IPlayerStartMovement, IDisposable
    {
        public event Action<string> OnChooseNode;
        public event Action<bool> OnSelectionStateChanged;
        public event Action<CityData, Vector3> OnCityPreview;

        private readonly CityViewSpawner _cityViewSpawner;

        private string _currentPlayerNode;
        private UniTaskCompletionSource<string> _tcs;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isChoosingTarget;
        private CityView _pendingCity;

        public bool IsChoosingTarget => _isChoosingTarget;

        public PlayerStartMovement(CityViewSpawner cityViewSpawner)
        {
            _cityViewSpawner = cityViewSpawner;
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
                UnsubscribeToNodes();
                _pendingCity = null;
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

        public void ConfirmSelection()
        {
            if (_pendingCity == null || _pendingCity.City == null) return;
            string nodeId = _pendingCity.City.NodeId;
            _pendingCity = null;
            _tcs?.TrySetResult(nodeId);
        }

        public void CancelPreview()
        {
            _pendingCity = null;
        }

        public void RequestCityPreview(CityData city)
        {
            if (city == null || !_isChoosingTarget)
                return;

            var view = _cityViewSpawner.FindByNodeId(city.NodeId);
            if (view == null)
                return;

            if (_pendingCity != null)
                return;

            _pendingCity = view;
            OnCityPreview?.Invoke(view.City, view.transform.position);
        }

        private void SubscribeToNodes()
        {
            foreach (CityView view in _cityViewSpawner.Views)
                view.OnClick += OnChooseNodeCollider;
        }

        private void UnsubscribeToNodes()
        {
            foreach (CityView view in _cityViewSpawner.Views)
                view.OnClick -= OnChooseNodeCollider;
        }

        private void OnChooseNodeCollider(IInteractableObject interactableObject)
        {
            if (interactableObject is not CityView view) return;
            if (view.City == null || _pendingCity != null)
                return;

            _pendingCity = view;
            OnCityPreview?.Invoke(view.City, view.transform.position);
        }

        private void SetSelectionState(bool state)
        {
            if (_isChoosingTarget == state)
                return;

            _isChoosingTarget = state;
            _cityViewSpawner.SetCollidersEnabled(state);
            OnSelectionStateChanged?.Invoke(state);
        }
    }
}
