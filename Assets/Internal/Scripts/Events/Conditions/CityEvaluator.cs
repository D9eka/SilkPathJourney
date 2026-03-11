using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player;
using UnityEngine;

namespace Internal.Scripts.Events.Conditions
{
    public class CityEvaluator : IConditionEvaluator
    {
        private const string PortAccessParam = "PortAccess";

        private static readonly EventConditionType[] Types =
        {
            EventConditionType.InCity,
            EventConditionType.CityModifier
        };

        private readonly IPlayerStateProvider _playerState;
        private readonly ICityNodeResolver _cityNodeResolver;

        public CityEvaluator(IPlayerStateProvider playerState, ICityNodeResolver cityNodeResolver)
        {
            _playerState = playerState;
            _cityNodeResolver = cityNodeResolver;
        }

        public IEnumerable<EventConditionType> SupportedTypes => Types;

        public bool Evaluate(EventCondition condition, PlayerResourceState resources)
        {
            string nodeId = _playerState.CurrentNodeId;
            if (string.IsNullOrEmpty(nodeId))
                return false;

            if (!_cityNodeResolver.TryGetCityByNodeId(nodeId, out CityData city))
                return false;

            if (condition.Type == EventConditionType.InCity)
                return true;

            return EvaluateCityModifier(city, condition.Param);
        }

        private static bool EvaluateCityModifier(CityData city, string param)
        {
            if (string.Equals(param, PortAccessParam, StringComparison.OrdinalIgnoreCase))
                return city.HasPort;

            Debug.LogWarning($"[SPJ Events] Unknown city modifier param: {param}");
            return false;
        }
    }
}
