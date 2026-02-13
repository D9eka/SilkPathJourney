using System;
using Internal.Scripts.Economy.Cities;
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
        [SerializeField] private BuildingData _data;
        [SerializeField] private Renderer[] _renderers;

        [Header("Outline")]
        [SerializeField] private Color _outlineColor = Color.yellow;
        [SerializeField] private float _outlineWidth = 0.03f;

        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        
        private WorldCanvas _worldCanvas;
        private ScreenStackService _screenStackService;
        private ICityNodeResolver _cityNodeResolver;
        private IPlayerStateProvider _playerStateProvider;
        private WorldLabelViewHelper _labelHelper;

        private MaterialPropertyBlock _propertyBlock;

        public event Action<IInteractableObject> OnClick;
        public BuildingData Data => _data;

        [Inject]
        public void Construct(WorldCanvas worldCanvas, ScreenStackService screenStackService,
            ICityNodeResolver cityNodeResolver, IPlayerStateProvider playerStateProvider)
        {
            _worldCanvas = worldCanvas;
            _screenStackService = screenStackService;
            _cityNodeResolver = cityNodeResolver;
            _playerStateProvider = playerStateProvider;
            _labelHelper = new WorldLabelViewHelper(worldCanvas);
        }

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            if (_renderers == null || _renderers.Length == 0)
                _renderers = GetComponentsInChildren<Renderer>();
        }

        private void Start()
        {
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
            SetOutline(true);
        }

        public void TriggerHoverExit()
        {
            SetOutline(false);
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

        private void SetOutline(bool enabled)
        {
            float width = enabled ? _outlineWidth : 0f;
            Color color = enabled ? _outlineColor : Color.clear;

            _propertyBlock.SetColor(OutlineColorId, color);
            _propertyBlock.SetFloat(OutlineWidthId, width);

            foreach (Renderer r in _renderers)
            {
                if (r != null)
                    r.SetPropertyBlock(_propertyBlock);
            }
        }

        private void OnDestroy()
        {
            _labelHelper?.Dispose();
        }
    }
}

