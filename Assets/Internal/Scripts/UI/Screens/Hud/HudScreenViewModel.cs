using System;
using Internal.Scripts.Camera;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events;
using Internal.Scripts.Player;
using Internal.Scripts.Quests;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.UI.Screens.Event;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.UI.Arrow.Controller;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.World.State;
using R3;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Hud
{
    public sealed class HudScreenViewModel : ScreenViewModelBase
    {
        private readonly HudModel _model;
        private readonly ScreenStackService _screenStackService;
        private readonly ICityEntryService _cityEntryService;
        private readonly IPlayerStateProvider _playerStateProvider;
        private readonly IPlayerMovementControl _playerMovementControl;
        private readonly IPlayerTurnChoiceState _turnChoiceState;
        private readonly IArrowsController _arrowsController;
        private readonly EventToastController _toastController;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly DayTracker _dayTracker;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly ResourceIconCatalog _iconCatalog;
        private readonly EventOutcomeFormatter _outcomeFormatter;
        private readonly CameraController _cameraController;
        private readonly QuestRepository _questRepository;
        private readonly QuestDatabase _questDatabase;

        private readonly ReactiveProperty<QuestTrackerState> _trackerState = new();
        private IDisposable _questSubscription;
        private string _prevTrackedQuestId;
        private int _prevStageIndex = -1;

        public event Action<bool> InteractableChanged;
        public event Action<bool> VisibilityChanged;
        public event Action<int> DayChanged;

        public HudScreenViewModel(
            HudModel model,
            ScreenStackService screenStackService,
            ICityEntryService cityEntryService,
            IPlayerStateProvider playerStateProvider,
            IPlayerMovementControl playerMovementControl,
            IPlayerTurnChoiceState turnChoiceState,
            IArrowsController arrowsController,
            EventToastController toastController,
            PlayerResourceRepository resourceRepository,
            DayTracker dayTracker,
            GameBalanceConfig balanceConfig,
            ResourceIconCatalog iconCatalog,
            EventOutcomeFormatter outcomeFormatter,
            CameraController cameraController,
            QuestRepository questRepository,
            QuestDatabase questDatabase)
        {
            _model = model;
            _screenStackService = screenStackService;
            _cityEntryService = cityEntryService;
            _playerStateProvider = playerStateProvider;
            _playerMovementControl = playerMovementControl;
            _turnChoiceState = turnChoiceState;
            _arrowsController = arrowsController;
            _toastController = toastController;
            _resourceRepository = resourceRepository;
            _dayTracker = dayTracker;
            _balanceConfig = balanceConfig;
            _iconCatalog = iconCatalog;
            _outcomeFormatter = outcomeFormatter;
            _cameraController = cameraController;
            _questRepository = questRepository;
            _questDatabase = questDatabase;
        }

        public void RegisterToastView(IEventToastView view)
        {
            if (view != null)
                _toastController.RegisterView(view);
        }

        public override ScreenId Id => ScreenId.Hud;

        public Observable<HudViewState> State => _model.State;
        public Observable<HudResourceViewState> Resources => _model.ResourceState;
        public Observable<TimeSpeed> TimeSpeedState => _model.TimeSpeedState;
        public Observable<QuestTrackerState> TrackerState => _trackerState;
        public float TimeScale => _model.TimeScale;
        public int CurrentDay => _dayTracker.CurrentDay;
        public ResourceIconCatalog ResourceIcons => _iconCatalog;
        public EventOutcomeFormatter OutcomeFormatter => _outcomeFormatter;

        protected override void OnOpen(object args)
        {
            _model.Activate();
            _dayTracker.OnDayChanged += HandleDayChanged;

            _prevTrackedQuestId = null;
            _prevStageIndex = -1;
            _questSubscription = _questRepository.Changed.Subscribe(_ => RebuildTrackerState());
        }

        protected override void OnClose()
        {
            _model.Deactivate();
            _dayTracker.OnDayChanged -= HandleDayChanged;

            _questSubscription?.Dispose();
            _questSubscription = null;
        }

        public override void OnFocusGained()
        {
            VisibilityChanged?.Invoke(true);
            InteractableChanged?.Invoke(true);
            RebuildTrackerState();
        }

        public override void OnFocusLost()
        {
            InteractableChanged?.Invoke(false);
            if (_screenStackService.TopId == ScreenId.TargetSelection)
                VisibilityChanged?.Invoke(false);
        }

        public void OnStartAction()
        {
            switch (_model.CurrentMode)
            {
                case HudMode.Travel:
                    _model.SetSpeed(0);
                    break;
                case HudMode.City:
                    if (_cityEntryService.IsInCityView)
                        StartMove();
                    else
                        EnterCity();
                    break;
            }
        }

        public void OnAction()
        {
            if (_model.CurrentMode == HudMode.Travel)
                _model.SetSpeed(1);
        }

        public void OnEndAction()
        {
            switch (_model.CurrentMode)
            {
                case HudMode.Travel:
                    _model.SetSpeed(2);
                    break;
                case HudMode.City:
                    StartMove();
                    break;
            }
        }

        public void OpenInventory()
        {
            if (!_screenStackService.TryOpen(ScreenId.Inventory, out ScreenOpenResult result))
                Debug.LogWarning($"[SPJ] Cannot open inventory screen: {result}");
        }

        public void OnTimeSpeedSelected(TimeSpeed speed)
        {
            _model.SetTimeSpeed(speed);
        }

        public void OpenPause()
        {
            if (!_screenStackService.TryOpen(ScreenId.Pause, out ScreenOpenResult result))
                Debug.LogWarning($"[SPJ] Cannot open pause screen: {result}");
        }

        public void OpenTrader()
        {
            if (!_screenStackService.TryOpen(ScreenId.Trader, out ScreenOpenResult result))
                Debug.LogWarning($"[SPJ] Cannot open trader screen: {result}");
        }

        public void OpenQuests()
        {
            if (!_screenStackService.TryOpen(ScreenId.Quests, out ScreenOpenResult result))
                Debug.LogWarning($"[SPJ] Cannot open quests screen: {result}");
        }

        public void OpenCaravan()
        {
            if (!_screenStackService.TryOpen(ScreenId.Caravan, out ScreenOpenResult result))
                Debug.LogWarning($"[SPJ] Cannot open caravan screen: {result}");
        }

        public void LockCameraToPlayer() => _cameraController.FollowPlayer();

        private void EnterCity()
        {
            if (_turnChoiceState.IsChoosingTurn)
            {
                _arrowsController.HideArrows();
                string turnNodeId = _turnChoiceState.CurrentTurnNodeId;
                if (!string.IsNullOrWhiteSpace(turnNodeId))
                    _playerMovementControl.CancelDestinationAtNode(turnNodeId);
            }

            if (!_model.TryGetEnterCity(out CityData city))
                return;

            _screenStackService.TryOpen(ScreenId.CityEntryConfirm, city, out _);
        }

        private void RebuildTrackerState()
        {
            string trackedId = _questRepository.GetTrackedQuestId();
            QuestData quest = string.IsNullOrEmpty(trackedId) ? null : _questDatabase.GetById(trackedId);
            int stageIndex = quest != null ? _questRepository.GetCurrentStageIndex(trackedId) : -1;

            TrackerChangeType changeType;

            if (quest == null && _prevTrackedQuestId == null)
            {
                return;
            }

            if (quest == null)
            {
                changeType = !string.IsNullOrEmpty(_prevTrackedQuestId) && _questRepository.IsFailed(_prevTrackedQuestId)
                    ? TrackerChangeType.QuestFailed
                    : TrackerChangeType.QuestCompleted;
            }
            else if (_prevTrackedQuestId == null)
            {
                changeType = TrackerChangeType.NewQuest;
            }
            else if (trackedId != _prevTrackedQuestId)
            {
                changeType = TrackerChangeType.QuestSwitched;
            }
            else if (stageIndex != _prevStageIndex && _prevStageIndex >= 0)
            {
                changeType = TrackerChangeType.StageAdvanced;
            }
            else
            {
                changeType = TrackerChangeType.None;
            }

            int prevStage = _prevStageIndex;
            QuestData displayQuest = quest ?? (
                !string.IsNullOrEmpty(_prevTrackedQuestId) ? _questDatabase.GetById(_prevTrackedQuestId) : null);
            _prevTrackedQuestId = trackedId;
            _prevStageIndex = stageIndex;

            _trackerState.Value = new QuestTrackerState
            {
                Quest = displayQuest,
                CurrentStageIndex = stageIndex,
                PreviousStageIndex = prevStage,
                ChangeType = changeType
            };
        }

        private void HandleDayChanged(int day) => DayChanged?.Invoke(day);

        private void StartMove()
        {
            if (_cityEntryService.IsInCityView)
            {
                _cityEntryService.ExitCity(() =>
                {
                    _screenStackService.TryOpen(ScreenId.TargetSelection, out _);
                });
                return;
            }

            _screenStackService.TryOpen(ScreenId.TargetSelection, out _);
        }
    }
}
