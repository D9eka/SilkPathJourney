using System;
using Internal.Scripts.Camera;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Player;
using Internal.Scripts.Player.StartMovement;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.TargetSelection.Search;
using Internal.Scripts.UI.StackService;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.TargetSelection
{
    public sealed class TargetSelectionScreenViewModel : ScreenViewModelBase
    {
        private readonly IPlayerStartMovement _playerStartMovement;
        private readonly ScreenStackService _screenStackService;
        private readonly CameraController _cameraController;
        private readonly CameraSceneSettings _settings;
        private readonly TravelEstimator _travelEstimator;
        private readonly CitySearchPanelViewModel _searchPanel;
        private readonly CityCardBuilder _cardBuilder;

        public event Action<CityData, TravelEstimate, CityRowData> PreviewChanged;

        public CitySearchPanelViewModel SearchPanel => _searchPanel;

        public TargetSelectionScreenViewModel(
            IPlayerStartMovement playerStartMovement,
            ScreenStackService screenStackService,
            CameraController cameraController,
            CameraSceneSettings settings,
            TravelEstimator travelEstimator,
            CitySearchPanelViewModel searchPanel,
            CityCardBuilder cardBuilder)
        {
            _playerStartMovement = playerStartMovement;
            _screenStackService = screenStackService;
            _cameraController = cameraController;
            _settings = settings;
            _travelEstimator = travelEstimator;
            _searchPanel = searchPanel;
            _cardBuilder = cardBuilder;
        }

        public override ScreenId Id => ScreenId.TargetSelection;

        protected override void OnOpen(object args)
        {
            _playerStartMovement.OnSelectionStateChanged += HandleSelectionStateChanged;
            _playerStartMovement.OnCityPreview += HandleCityPreview;
            _playerStartMovement.BeginSelection();
            _cameraController.UnfollowPlayer();
            _cameraController.ZoomCamera(_settings.TargetSelectionZoomSize);

            _searchPanel.Activate();
            _searchPanel.CityRowClicked += HandleSearchCityClicked;
        }

        protected override void OnClose()
        {
            _searchPanel.CityRowClicked -= HandleSearchCityClicked;
            _searchPanel.Deactivate();

            _playerStartMovement.OnSelectionStateChanged -= HandleSelectionStateChanged;
            _playerStartMovement.OnCityPreview -= HandleCityPreview;
            if (_playerStartMovement.IsChoosingTarget)
                _playerStartMovement.CancelSelection();
            _cameraController.FollowPlayer();
        }

        public void Cancel()
        {
            _screenStackService.Close(ScreenId.TargetSelection);
        }

        public void ConfirmTarget()
        {
            _playerStartMovement.ConfirmSelection();
        }

        public void CancelPreview()
        {
            _playerStartMovement.CancelPreview();
            _cameraController.ZoomCamera(_settings.TargetSelectionZoomSize);
            _searchPanel.SetVisible(true);
            PreviewChanged?.Invoke(null, default, default);
        }

        private void HandleCityPreview(CityData city, Vector3 worldPos)
        {
            Vector2 targetXZ = new Vector2(worldPos.x, worldPos.z);
            Vector2 cameraXZ = _cameraController.CameraTransform.GetWorldTarget();
            float distance = Vector2.Distance(cameraXZ, targetXZ);
            float moveDuration = Mathf.Max(distance / _settings.PreviewMoveSpeed, _settings.FollowTransitionDuration);
            _cameraController.MoveCamera(targetXZ, moveDuration);
            _cameraController.ZoomCamera(_settings.FollowZoomSize);

            TravelEstimate estimate = _travelEstimator.Estimate(city.NodeId);
            CityRowData rowData = _cardBuilder.Build(city);
            _searchPanel.SetVisible(false);
            PreviewChanged?.Invoke(city, estimate, rowData);
        }

        private void HandleSelectionStateChanged(bool isSelecting)
        {
            if (!isSelecting)
                _screenStackService.Close(ScreenId.TargetSelection);
        }

        private void HandleSearchCityClicked(CityData city)
        {
            _playerStartMovement.RequestCityPreview(city);
        }
    }
}
