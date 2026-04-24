using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Events;
using Internal.Scripts.Player;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Travel.Triggers
{
    public sealed class SegmentTriggerService : IFixedTickable
    {
        private readonly IReadOnlyList<ISegmentTriggerAction> _actions;
        private readonly PlayerController _player;
        private readonly GameBalanceConfig _balance;
        private readonly CaravanSpeedService _speed;
        private readonly DayTracker _dayTracker;
        private readonly ScreenStackService _screens;
        private readonly PlayerResourceRepository _resourceRepository;

        private Vector3? _lastPos;
        private float _accumulator;
        private ISegmentTriggerAction _lastAction;

        public SegmentTriggerService(
            List<ISegmentTriggerAction> actions,
            PlayerController player,
            GameBalanceConfig balance,
            CaravanSpeedService speed,
            DayTracker dayTracker,
            ScreenStackService screens,
            PlayerResourceRepository resourceRepository)
        {
            _actions = actions;
            _player = player;
            _balance = balance;
            _speed = speed;
            _dayTracker = dayTracker;
            _screens = screens;
            _resourceRepository = resourceRepository;
        }

        public void FixedTick()
        {
            if (!GlobalAllow())
            {
                _lastPos = null;
                return;
            }

            Vector3 pos = _player.CurrentPosition;
            if (_lastPos.HasValue)
                _accumulator += Vector3.Distance(_lastPos.Value, pos);
            _lastPos = pos;

            float interval = CurrentInterval();
            while (_accumulator >= interval)
            {
                _accumulator -= interval;
                PickAndFire();
                interval = CurrentInterval();
            }
        }

        private float CurrentInterval()
        {
            float danger = _resourceRepository.Current.AccumulatedDanger;
            float maxDanger = _balance.MaxDanger;
            float scale = 1f - (danger / maxDanger) * _balance.HazardDangerMultiplier;
            return Mathf.Max(_balance.SegmentTriggerIntervalMeters * scale, 0.5f);
        }

        private bool GlobalAllow()
        {
            if (_player.State != PlayerState.Moving) return false;
            if (_speed.CurrentMode.Value == CaravanSpeedMode.Camp) return false;
            if (_dayTracker.IsSkipping) return false;
            if (_screens.IsOpen(ScreenId.Event)) return false;
            if (_screens.IsOpen(ScreenId.Trade)) return false;
            if (_screens.IsOpen(ScreenId.Camp)) return false;
            return true;
        }

        private void PickAndFire()
        {
            var candidates = _actions.Where(a => a.CanTrigger() && a.Weight > 0).ToList();
            if (candidates.Count == 0) return;

            var pool = candidates.Count > 1 && _lastAction != null
                ? candidates.Where(a => a != _lastAction).ToList()
                : candidates;

            int total = pool.Sum(a => a.Weight);
            if (total <= 0) return;

            int roll = UnityEngine.Random.Range(0, total);
            foreach (ISegmentTriggerAction action in pool)
            {
                roll -= action.Weight;
                if (roll < 0)
                {
                    _lastAction = action;
                    action.Trigger();
                    return;
                }
            }
        }
    }
}
