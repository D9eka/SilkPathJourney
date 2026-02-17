using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.InteractableObjects;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Nodes;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.WorldLabel;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Economy.Buildings
{
    public class BuildingView : MonoBehaviour, IInteractableObject
    {
        [SerializeField] private BuildingId _buildingId;
        [SerializeField] private Renderer[] _renderers;

        [Header("Outline")]
        [SerializeField] private Color _outlineColor = Color.yellow;

        private ScreenStackService _screenStackService;
        private ICityNodeResolver _cityNodeResolver;
        private IPlayerStateProvider _playerStateProvider;
        private EconomyDatabase _economyDatabase;
        private WorldLabelViewHelper _labelHelper;

        private InteractableOutline _outline;
        private BuildingData _data;

        public event Action<IInteractableObject> OnClick;
        public BuildingData Data => _data;
        public BuildingId BuildingId => _buildingId;

        [Inject]
        public void Construct(WorldCanvas worldCanvas, ScreenStackService screenStackService,
            ICityNodeResolver cityNodeResolver, IPlayerStateProvider playerStateProvider,
            EconomyDatabase economyDatabase)
        {
            _labelHelper = new WorldLabelViewHelper(worldCanvas);
            _screenStackService = screenStackService;
            _cityNodeResolver = cityNodeResolver;
            _playerStateProvider = playerStateProvider;
            _economyDatabase = economyDatabase;
        }

        private void Awake()
        {
            if (_renderers == null || _renderers.Length == 0)
                _renderers = GetComponentsInChildren<Renderer>();
            _outline = new InteractableOutline(_renderers, _outlineColor);
        }

        private void Start()
        {
            _data = _economyDatabase.GetBuilding(_buildingId);
            if (_data == null) return;

            _labelHelper.CreateLabel(
                transform.position,
                $"BuildingLabel_{_data.Id}",
                _data.Name,
                _data.Id,
                _data);
        }

        public void TriggerHoverEnter()
        {
            _outline.Show();
        }

        public void TriggerHoverExit()
        {
            _outline.Hide();
        }

        public void TriggerClick()
        {
            OnClick?.Invoke(this);

            if (_data == null || _data.InteractionScreen == ScreenId.None) return;
            if (_screenStackService == null) return;

            string cityId = ResolveCityId();
            _screenStackService.TryOpen(_data.InteractionScreen, cityId, out ScreenOpenResult _);
        }

        private string ResolveCityId()
        {
            string nodeId = _playerStateProvider?.CurrentNodeId;
            if (!string.IsNullOrWhiteSpace(nodeId) &&
                _cityNodeResolver.TryGetCityByNodeId(nodeId, out CityData city))
                return city.Id;
            return null;
        }

        private void OnDestroy()
        {
            _outline?.Dispose();
            _labelHelper?.Dispose();
        }
    }
}

