using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Player;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Player.StartMovement;
using Internal.Scripts.World.State;
using R3;

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
        private readonly ReactiveProperty<HudViewState> _state;

        private int _activeSpeedIndex = 1;

        public HudModel(
            IPlayerStateProvider playerStateProvider,
            IPlayerStateEvents playerStateEvents,
            ICityNodeResolver cityNodeResolver,
            IPlayerStartMovement playerStartMovement,
            IPlayerTurnChoiceState turnChoiceState,
            WorldStateController worldStateController)
        {
            _playerStateProvider = playerStateProvider;
            _playerStateEvents = playerStateEvents;
            _cityNodeResolver = cityNodeResolver;
            _playerStartMovement = playerStartMovement;
            _turnChoiceState = turnChoiceState;
            _worldStateController = worldStateController;
            _state = new ReactiveProperty<HudViewState>(
                new HudViewState(HudMode.Travel, _activeSpeedIndex, null));
        }

        public Observable<HudViewState> State => _state;

        public HudMode CurrentMode => _state.Value.Mode;

        public void Activate()
        {
            _playerStateEvents.OnCurrentNodeChanged += HandleStateChanged;
            _playerStateEvents.OnDestinationChanged += HandleStateChanged;
            _playerStartMovement.OnSelectionStateChanged += HandleSelectionChanged;
            _turnChoiceState.OnTurnChoiceStateChanged += HandleTurnChoiceChanged;
            _worldStateController.OnStateChange += HandleViewModeChanged;
            UpdateState();
        }

        public void Deactivate()
        {
            _playerStateEvents.OnCurrentNodeChanged -= HandleStateChanged;
            _playerStateEvents.OnDestinationChanged -= HandleStateChanged;
            _playerStartMovement.OnSelectionStateChanged -= HandleSelectionChanged;
            _turnChoiceState.OnTurnChoiceStateChanged -= HandleTurnChoiceChanged;
            _worldStateController.OnStateChange -= HandleViewModeChanged;
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
