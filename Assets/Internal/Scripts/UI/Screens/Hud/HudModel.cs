using System;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Player;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Player.StartMovement;
using Internal.Scripts.UI.Components;
using Internal.Scripts.World.State;
using R3;
using UnityEngine;

namespace Internal.Scripts.Hud
{
    public sealed class HudModel
    {
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IPlayerStateEvents _playerStateEvents;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly IPlayerStartMovement _playerStartMovement;
        private readonly IPlayerTurnChoiceState _turnChoiceState;
        private readonly WorldStateController _worldStateController;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly ResourceIconCatalog _iconCatalog;
        private readonly ReactiveProperty<HudViewState> _state;
        private readonly ReactiveProperty<HudResourceViewState> _resourceState = new();

        private int _activeSpeedIndex = 1;
        private float _prevFood = -1f;
        private float _prevDanger = -1f;
        private float _prevPlayerCartDur = -1f;
        private IDisposable _resourceSubscription;

        public HudModel(
            IPlayerStateProvider playerStateProvider,
            IPlayerStateEvents playerStateEvents,
            ICityNodeResolver cityNodeResolver,
            IPlayerStartMovement playerStartMovement,
            IPlayerTurnChoiceState turnChoiceState,
            WorldStateController worldStateController,
            PlayerResourceRepository resourceRepository,
            GameBalanceConfig balanceConfig,
            ResourceIconCatalog iconCatalog)
        {
            _playerStateProvider = playerStateProvider;
            _playerStateEvents = playerStateEvents;
            _cityNodeResolver = cityNodeResolver;
            _playerStartMovement = playerStartMovement;
            _turnChoiceState = turnChoiceState;
            _worldStateController = worldStateController;
            _resourceRepository = resourceRepository;
            _balanceConfig = balanceConfig;
            _iconCatalog = iconCatalog;
            _state = new ReactiveProperty<HudViewState>(
                new HudViewState(HudMode.Travel, _activeSpeedIndex, null));
        }

        public Observable<HudViewState> State => _state;
        public Observable<HudResourceViewState> ResourceState => _resourceState;

        public HudMode CurrentMode => _state.Value.Mode;

        public void Activate()
        {
            _prevFood = -1f;
            _prevDanger = -1f;
            _prevPlayerCartDur = -1f;

            _playerStateEvents.OnCurrentNodeChanged += HandleStateChanged;
            _playerStateEvents.OnDestinationChanged += HandleStateChanged;
            _playerStartMovement.OnSelectionStateChanged += HandleSelectionChanged;
            _turnChoiceState.OnTurnChoiceStateChanged += HandleTurnChoiceChanged;
            _worldStateController.OnStateChange += HandleViewModeChanged;
            _resourceSubscription = _resourceRepository.StateStream.Subscribe(ComputeResourceState);
            UpdateState();
        }

        public void Deactivate()
        {
            _playerStateEvents.OnCurrentNodeChanged -= HandleStateChanged;
            _playerStateEvents.OnDestinationChanged -= HandleStateChanged;
            _playerStartMovement.OnSelectionStateChanged -= HandleSelectionChanged;
            _turnChoiceState.OnTurnChoiceStateChanged -= HandleTurnChoiceChanged;
            _worldStateController.OnStateChange -= HandleViewModeChanged;
            _resourceSubscription?.Dispose();
            _resourceSubscription = null;
        }

        public void SetSpeed(int index)
        {
            _activeSpeedIndex = index;
            UpdateState();
        }

        public bool TryGetEnterCity(out CityData city)
        {
            city = null;
            string nodeId = ResolveNodeIdForCity();
            if (string.IsNullOrWhiteSpace(nodeId))
                return false;

            return _cityNodeResolver.TryGetCityByNodeId(nodeId, out city);
        }

        private void ComputeResourceState(PlayerResourceState res)
        {
            float food = res.Food;
            bool foodAnimate = _prevFood >= 0f && !Mathf.Approximately(food, _prevFood);
            int foodChange = foodAnimate ? Mathf.RoundToInt(food - _prevFood) : 0;
            _prevFood = food;

            float dur = res.PlayerCart.Durability;
            float maxDur = res.PlayerCart.MaxDurability;
            bool durAnimate = _prevPlayerCartDur >= 0f && !Mathf.Approximately(dur, _prevPlayerCartDur);
            int durChange = durAnimate ? Mathf.RoundToInt(dur - _prevPlayerCartDur) : 0;
            _prevPlayerCartDur = dur;

            bool showOtherCarts = res.Carts.Count > 0;
            float otherDur = 0f;
            float otherMax = 0f;
            if (showOtherCarts)
            {
                float totalDur = 0f;
                float totalMax = 0f;
                foreach (CartState c in res.Carts)
                {
                    totalDur += c.Durability;
                    totalMax += c.MaxDurability;
                }
                otherDur = totalDur / res.Carts.Count;
                otherMax = totalMax / res.Carts.Count;
            }

            float danger = res.AccumulatedDanger;
            float maxDanger = _balanceConfig.MaxDanger;
            bool dangerAnimate = _prevDanger >= 0f && !Mathf.Approximately(danger, _prevDanger);
            int dangerChange = dangerAnimate ? Mathf.RoundToInt(danger - _prevDanger) : 0;
            _prevDanger = danger;

            _resourceState.Value = new HudResourceViewState(
                new ResourceIndicatorState(food, 0f, foodChange, foodAnimate,
                    GetIncreaseIsPositive(ResourceType.Food)),
                new ResourceIndicatorState(dur, maxDur, durChange, durAnimate,
                    GetIncreaseIsPositive(ResourceType.PlayerCartDurability)),
                new ResourceIndicatorState(otherDur, otherMax, 0, false,
                    GetIncreaseIsPositive(ResourceType.OtherCartsDurability), showOtherCarts),
                new ResourceIndicatorState(danger, maxDanger, dangerChange, dangerAnimate,
                    GetIncreaseIsPositive(ResourceType.Danger))
            );
        }

        private bool GetIncreaseIsPositive(ResourceType type) =>
            _iconCatalog?.Get(type)?.IncreaseIsPositive ?? true;

        private void HandleStateChanged(string _) => UpdateState();

        private void HandleSelectionChanged(bool _) => UpdateState();

        private void HandleTurnChoiceChanged(bool _) => UpdateState();

        private void HandleViewModeChanged(WorldViewMode _) => UpdateState();

        private void UpdateState()
        {
            HudMode mode = DetermineMode();
            CityData city = null;
            if (mode == HudMode.CityDetailed)
                TryGetEnterCity(out city);

            _state.Value = new HudViewState(mode, _activeSpeedIndex, city);
        }

        private HudMode DetermineMode()
        {
            PlayerState playerState = _playerStateProvider.State;
            if (playerState == PlayerState.Moving)
                return HudMode.Travel;

            string nodeId = ResolveNodeIdForCity();
            if (!string.IsNullOrWhiteSpace(nodeId) &&
                _cityNodeResolver.TryGetCityByNodeId(nodeId, out _))
            {
                return _worldStateController.CurrentViewMode == WorldViewMode.CityIso
                    ? HudMode.CityDetailed
                    : HudMode.CityStrategic;
            }

            return HudMode.Travel;
        }

        private string ResolveNodeIdForCity()
        {
            if (_turnChoiceState.IsChoosingTurn)
                return _turnChoiceState.CurrentTurnNodeId;

            return _playerStateProvider.CurrentNodeId;
        }
    }
}
