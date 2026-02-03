using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Player;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Player.StartMovement;
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
        private readonly ReactiveProperty<HudViewState> _state;

        public HudModel(
            IPlayerStateProvider playerStateProvider,
            IPlayerStateEvents playerStateEvents,
            ICityNodeResolver cityNodeResolver,
            IPlayerStartMovement playerStartMovement,
            IPlayerTurnChoiceState turnChoiceState)
        {
            _playerStateProvider = playerStateProvider;
            _playerStateEvents = playerStateEvents;
            _cityNodeResolver = cityNodeResolver;
            _playerStartMovement = playerStartMovement;
            _turnChoiceState = turnChoiceState;
            _state = new ReactiveProperty<HudViewState>(new HudViewState(false, false, false));
        }

        public Observable<HudViewState> State => _state;

        public void Activate()
        {
            _playerStateEvents.OnCurrentNodeChanged += HandleStateChanged;
            _playerStateEvents.OnDestinationChanged += HandleStateChanged;
            _playerStartMovement.OnSelectionStateChanged += HandleSelectionChanged;
            _turnChoiceState.OnTurnChoiceStateChanged += HandleTurnChoiceChanged;
            UpdateState();
        }

        public void Deactivate()
        {
            _playerStateEvents.OnCurrentNodeChanged -= HandleStateChanged;
            _playerStateEvents.OnDestinationChanged -= HandleStateChanged;
            _playerStartMovement.OnSelectionStateChanged -= HandleSelectionChanged;
            _turnChoiceState.OnTurnChoiceStateChanged -= HandleTurnChoiceChanged;
        }

        public bool TryGetEnterCity(out CityData city)
        {
            city = null;
            string nodeId = ResolveNodeIdForEnterCity();
            if (string.IsNullOrWhiteSpace(nodeId))
                return false;

            return _cityNodeResolver.TryGetCityByNodeId(nodeId, out city);
        }

        private void HandleStateChanged(string _)
        {
            UpdateState();
        }

        private void HandleSelectionChanged(bool _)
        {
            UpdateState();
        }

        private void HandleTurnChoiceChanged(bool _)
        {
            UpdateState();
        }

        private void UpdateState()
        {
            bool isSelectingTarget = _playerStartMovement.IsChoosingTarget;
            bool isMoving = _playerStateProvider.State == PlayerState.Moving;
            bool showStart = !isSelectingTarget && !isMoving;
            bool showCancel = isSelectingTarget;
            bool showEnter = CanEnterCity();
            _state.Value = new HudViewState(showStart, showCancel, showEnter);
        }

        private bool CanEnterCity()
        {
            PlayerState state = _playerStateProvider.State;
            if (state == PlayerState.SelectingTarget)
                return false;

            if (state != PlayerState.Idle && !_turnChoiceState.IsChoosingTurn)
                return false;

            string nodeId = ResolveNodeIdForEnterCity();
            if (string.IsNullOrWhiteSpace(nodeId))
                return false;

            return _cityNodeResolver.TryGetCityByNodeId(nodeId, out _);
        }

        private string ResolveNodeIdForEnterCity()
        {
            if (_turnChoiceState.IsChoosingTurn)
                return _turnChoiceState.CurrentTurnNodeId;

            return _playerStateProvider.CurrentNodeId;
        }
    }
}
