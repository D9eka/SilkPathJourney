using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player;
using Internal.Scripts.WorldModifiers;
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
        private readonly WorldModifierRepository _modifierRepo;

        public CityEvaluator(
            IPlayerStateProvider playerState,
            ICityNodeResolver cityNodeResolver,
            WorldModifierRepository modifierRepo)
        {
            _playerState = playerState;
            _cityNodeResolver = cityNodeResolver;
            _modifierRepo = modifierRepo;
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

        private bool EvaluateCityModifier(CityData city, string param)
        {
            if (string.Equals(param, PortAccessParam, StringComparison.OrdinalIgnoreCase))
                return city.HasPort;

            return _modifierRepo.GetCityModifiers(city.Id)
                .Any(m => string.Equals(m.ModifierId, param, StringComparison.OrdinalIgnoreCase));
        }
    }
}
