using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Events;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.UI.Screens.Core.Config;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Travel.Hazards
{
    public sealed class HazardTrigger : IInitializable, IFixedTickable, IDisposable
    {
        private readonly PlayerController _player;
        private readonly IPlayerStateEvents _playerEvents;
        private readonly HazardSelector _selector;
        private readonly HazardController _controller;
        private readonly IRoadNetwork _roadNetwork;
        private readonly GameBalanceConfig _balance;
        private readonly PlayerResourceRepository _resourceRepository;
        private readonly ScreenStackService _screenStackService;
        private readonly DayTracker _dayTracker;
        private readonly CaravanSpeedService _caravanSpeedService;

        private readonly List<float> _triggerPoints = new();
        private int _nextTriggerIndex;

        public HazardTrigger(
            PlayerController player,
            IPlayerStateEvents playerEvents,
            HazardSelector selector,
            HazardController controller,
            IRoadNetwork roadNetwork,
            GameBalanceConfig balance,
            PlayerResourceRepository resourceRepository,
            ScreenStackService screenStackService,
            DayTracker dayTracker,
            CaravanSpeedService caravanSpeedService)
        {
            _player = player;
            _playerEvents = playerEvents;
            _selector = selector;
            _controller = controller;
            _roadNetwork = roadNetwork;
            _balance = balance;
            _resourceRepository = resourceRepository;
            _screenStackService = screenStackService;
            _dayTracker = dayTracker;
            _caravanSpeedService = caravanSpeedService;
        }

        public void Initialize()
        {
            _playerEvents.OnCurrentSegmentChanged += OnSegmentChanged;
        }

        public void Dispose()
        {
            _playerEvents.OnCurrentSegmentChanged -= OnSegmentChanged;
        }

        public void FixedTick()
        {
            if (_player.State != PlayerState.Moving)
                return;

            if (_caravanSpeedService.CurrentMode.Value == CaravanSpeedMode.Camp)
                return;

            if (_dayTracker.IsSkipping)
                return;

            if (_screenStackService.IsOpen(ScreenId.Event) ||
                _screenStackService.IsOpen(ScreenId.Trade) ||
                _screenStackService.IsOpen(ScreenId.Camp))
                return;

            if (_triggerPoints.Count == 0 || _nextTriggerIndex >= _triggerPoints.Count)
                return;

            float dist = _player.DistanceOnSegment;

            while (_nextTriggerIndex < _triggerPoints.Count &&
                   dist >= _triggerPoints[_nextTriggerIndex])
            {
                _nextTriggerIndex++;

                if (_controller.IsActive)
                    continue;

                HazardData hazard = _selector.SelectHazard();
                if (hazard != null)
                    _controller.Show(hazard);
            }
        }

        private void OnSegmentChanged(string fromNode, string toNode)
        {
            _triggerPoints.Clear();
            _nextTriggerIndex = 0;

            if (string.IsNullOrEmpty(fromNode) || string.IsNullOrEmpty(toNode))
                return;

            if (!_roadNetwork.TryGetSegment(fromNode, toNode, out RoadPathSegment segment))
                return;

            float length = segment.LengthMeters;
            if (length < _balance.MinSegmentLengthForHazard)
                return;

            float danger = _resourceRepository.Current.AccumulatedDanger;
            float maxDanger = _balance.MaxDanger;
            float interval = _balance.HazardBaseIntervalMeters *
                             (1f - (danger / maxDanger) * _balance.HazardDangerMultiplier);
            interval = Mathf.Max(interval, 5f);

            for (float dist = interval; dist < length - interval * 0.5f; dist += interval)
            {
                if (UnityEngine.Random.value > 0.5f)
                    _triggerPoints.Add(dist);
            }
        }
    }
}
