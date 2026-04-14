using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.WorldModifiers;
using UnityEngine;

namespace Internal.Scripts.Camp
{
    public sealed class CampActionService
    {
        private readonly CampActionDatabase _database;
        private readonly OutcomeApplier _outcomeApplier;
        private readonly InventoryRepository _inventoryRepository;
        private readonly PlayerSkillRepository _skillRepository;
        private readonly IPlayerStateProvider _playerState;
        private readonly ICityNodeResolver _cityNodeResolver;
        private readonly GameBalanceConfig _balanceConfig;

        private readonly Dictionary<CampActionType, int> _repeatCounts = new();

        public CampActionService(
            CampActionDatabase database,
            OutcomeApplier outcomeApplier,
            InventoryRepository inventoryRepository,
            PlayerSkillRepository skillRepository,
            IPlayerStateProvider playerState,
            ICityNodeResolver cityNodeResolver,
            GameBalanceConfig balanceConfig)
        {
            _database = database;
            _outcomeApplier = outcomeApplier;
            _inventoryRepository = inventoryRepository;
            _skillRepository = skillRepository;
            _playerState = playerState;
            _cityNodeResolver = cityNodeResolver;
            _balanceConfig = balanceConfig;
        }

        public void OnSegmentChanged()
        {
            _repeatCounts.Clear();
        }

        public int GetRepeatCount(CampActionType type)
            => _repeatCounts.TryGetValue(type, out int count) ? count : 0;

        public CampActionPreview GetPreview(CampActionType type)
        {
            var data = _database.GetAction(type);
            if (data == null)
                return new CampActionPreview { IsAvailable = false };

            float cost = data.CostSupplies;
            int repeatCount = GetRepeatCount(type);
            bool canAfford = GetSuppliesCount() >= cost;
            bool withinRepeatLimit = data.MaxRepeatPerSegment <= 0 || repeatCount < data.MaxRepeatPerSegment;

            return new CampActionPreview
            {
                ExpectedEffect = CalculateEffect(type, data),
                Cost = cost,
                IsAvailable = canAfford && withinRepeatLimit,
                RepeatDays = repeatCount
            };
        }

        public bool ExecuteAction(CampActionType type)
        {
            var data = _database.GetAction(type);
            if (data == null) return false;

            if (GetSuppliesCount() < data.CostSupplies) return false;

            if (data.CostSupplies > 0)
                _outcomeApplier.Apply(new EventOutcomeEntry(EventOutcomeType.Food, null, -data.CostSupplies));

            float effect = CalculateEffect(type, data);
            _outcomeApplier.Apply(new EventOutcomeEntry(data.AffectedResource, null, effect));

            IncrementRepeat(type);

            var sideEffect = GetSideEffectForRepeat(type);
            if (sideEffect.HasValue && sideEffect.Value.Resource != EventOutcomeType.None)
                _outcomeApplier.Apply(new EventOutcomeEntry(sideEffect.Value.Resource, null, sideEffect.Value.Value));

            foreach (var key in new List<CampActionType>(_repeatCounts.Keys))
            {
                if (key != type && key != CampActionType.Forage)
                    _repeatCounts[key] = 0;
            }

            return true;
        }

        private int GetSuppliesCount()
            => InventoryStateMutator.GetItemCount(_inventoryRepository.GetPlayerInventory(), SuppliesItemId.Value);

        public RepeatSideEffect? GetSideEffectForRepeat(CampActionType type)
        {
            var data = _database.GetAction(type);
            if (data == null || data.SideEffects == null || data.SideEffects.Count == 0)
                return null;

            int repeat = GetRepeatCount(type);
            RepeatSideEffect? best = null;

            foreach (var effect in data.SideEffects)
            {
                if (effect.RepeatDay <= repeat)
                    best = effect;
            }

            return best;
        }

        private void IncrementRepeat(CampActionType type)
        {
            _repeatCounts.TryGetValue(type, out int current);
            _repeatCounts[type] = current + 1;
        }

        private float CalculateEffect(CampActionType type, CampActionData data)
        {
            Biome biome = _cityNodeResolver.ResolveRoadBiome(
                _playerState.CurrentFromNodeId, _playerState.CurrentToNodeId);

            float biomeModifier = _database.GetBiomeModifier(biome, type);
            float skillBonus = GetSkillBonus(data);
            float diminishing = GetDiminishingFactor(data, GetRepeatCount(type));

            return data.BaseEffect * (1f + biomeModifier / 100f) * (1f + skillBonus) * diminishing;
        }

        private float GetSkillBonus(CampActionData data)
        {
            if (data.RelatedSkill == SkillType.None) return 0f;

            return _skillRepository.Current.GetSkill(data.RelatedSkill) * _balanceConfig.CampSkillEffectPerPoint;
        }

        private static float GetDiminishingFactor(CampActionData data, int repeatCount)
        {
            var curve = data.DiminishingCurve;
            if (curve == null || curve.Length == 0) return 1f;

            int index = Mathf.Clamp(repeatCount, 0, curve.Length - 1);
            return curve[index];
        }
    }
}
