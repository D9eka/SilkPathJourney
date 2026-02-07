using System;
using Internal.Scripts.Config;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Save;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Events
{
    public class DayTracker : IDisposable
    {
        public event Action<int> OnDayChanged;

        private readonly SaveRepository _saveRepository;
        private readonly float _metersPerDay;

        private RoadAgent _playerRoadAgent;
        private float _lastDistanceCheckpoint = 0f;

        public int CurrentDay => _saveRepository.Data.Player.CurrentDay;

        public DayTracker(SaveRepository saveRepository, GameBalanceConfig balanceConfig,
            RoadAgentConfig agentConfig)
        {
            _saveRepository = saveRepository;
            _metersPerDay = balanceConfig.SecondsPerDay * agentConfig.SpeedMetersPerSecond;
        }

        public void Initialize(RoadAgent playerRoadAgent)
        {
            _playerRoadAgent = playerRoadAgent;
            _playerRoadAgent.OnDistanceTraveled += HandleDistanceTraveled;
        }

        public void Dispose()
        {
            _playerRoadAgent.OnDistanceTraveled -= HandleDistanceTraveled;
        }

        private void HandleDistanceTraveled(float totalDistance)
        {
            float distanceSinceLastDay = totalDistance - _lastDistanceCheckpoint;

            if (distanceSinceLastDay >= _metersPerDay)
            {
                int daysToAdd = Mathf.FloorToInt(distanceSinceLastDay / _metersPerDay);
                _lastDistanceCheckpoint += daysToAdd * _metersPerDay;

                for (int i = 0; i < daysToAdd; i++)
                {
                    _saveRepository.Data.Player.CurrentDay++;
                    _saveRepository.Save();
                    OnDayChanged?.Invoke(CurrentDay);
                }
            }
        }
    }
}
