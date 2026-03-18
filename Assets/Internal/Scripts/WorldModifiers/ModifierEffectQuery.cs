using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Road.Modifiers;

namespace Internal.Scripts.WorldModifiers
{
    public sealed class ModifierEffectQuery
    {
        private readonly WorldModifierRepository _repo;
        private readonly EconomyDatabase _economyDb;

        public ModifierEffectQuery(WorldModifierRepository repo, EconomyDatabase economyDb)
        {
            _repo = repo;
            _economyDb = economyDb;
        }

        public float GetCityPriceMultiplier(string cityId)
            => AggregateCityEffect(cityId, d => d.PricePct);

        public float GetCityDangerMultiplier(string cityId)
            => AggregateCityEffect(cityId, d => d.DangerPct);

        public float GetRoadSpeedMultiplier(string roadId)
            => AggregateRoadEffect(roadId, d => d.SpeedPct);

        public float GetRoadSuppliesMultiplier(string roadId)
            => AggregateRoadEffect(roadId, d => d.SuppliesPct);

        public float GetRoadDangerMultiplier(string roadId)
            => AggregateRoadEffect(roadId, d => d.DangerPct);

        private const float PctDivisor = 100f;

        private float AggregateCityEffect(string cityId, Func<CityModifierData, float> selector)
        {
            float result = 1f;
            List<ActiveModifierEntry> entries = _repo.GetCityModifiers(cityId);
            for (int i = 0; i < entries.Count; i++)
            {
                var data = _economyDb.GetCityModifier(entries[i].ModifierId);
                if (data != null)
                    result *= 1f + selector(data) / PctDivisor;
            }
            return result;
        }

        private float AggregateRoadEffect(string roadId, Func<RoadModifierData, float> selector)
        {
            float result = 1f;
            List<ActiveModifierEntry> entries = _repo.GetRoadModifiers(roadId);
            for (int i = 0; i < entries.Count; i++)
            {
                var data = _economyDb.GetRoadModifier(entries[i].ModifierId);
                if (data != null)
                    result *= 1f + selector(data) / PctDivisor;
            }
            return result;
        }
    }
}
