using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using UnityEngine;

namespace Internal.Scripts.Camp
{
    public sealed class CampActionService
    {
        private readonly CampActionDatabase _database;
        private readonly OutcomeApplier _outcomeApplier;
        private readonly InventoryRepository _inventoryRepository;

        private readonly Dictionary<CampActionType, int> _repeatCounts = new();

        private CampActionType? _currentAction;
        public CampActionType? CurrentAction => _currentAction;

        public CampActionService(
            CampActionDatabase database,
            OutcomeApplier outcomeApplier,
            InventoryRepository inventoryRepository)
        {
            _database = database;
            _outcomeApplier = outcomeApplier;
            _inventoryRepository = inventoryRepository;
        }

        public void OnSegmentChanged()
        {
            _repeatCounts.Clear();
            _currentAction = null;
        }

        public void ClearCurrentAction() => _currentAction = null;

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

            IncrementRepeat(type);

            var sideEffect = GetSideEffectForRepeat(type);
            if (sideEffect.HasValue && sideEffect.Value.Resource != EventOutcomeType.None)
                _outcomeApplier.Apply(new EventOutcomeEntry(sideEffect.Value.Resource, null, sideEffect.Value.Value));

            _currentAction = type;
            return true;
        }

        private int GetSuppliesCount()
            => InventoryStateMutator.GetItemCount(_inventoryRepository.GetPlayerInventory(), SuppliesItemId.Value);

        public RepeatSideEffect? GetSideEffectForRepeat(CampActionType type)
            => GetSideEffectForRepeatDay(type, GetRepeatCount(type));

        private RepeatSideEffect? GetSideEffectForRepeatDay(CampActionType type, int repeatDay)
        {
            var data = _database.GetAction(type);
            if (data == null || data.SideEffects == null || data.SideEffects.Count == 0)
                return null;

            RepeatSideEffect? best = null;
            foreach (var effect in data.SideEffects)
            {
                if (effect.RepeatDay <= repeatDay)
                    best = effect;
            }
            return best;
        }

        public float GetCurrentDiminishingMultiplier(CampActionType type)
        {
            var data = _database.GetAction(type);
            if (data?.DiminishingCurve == null || data.DiminishingCurve.Length == 0) return 1f;
            int count = GetRepeatCount(type);
            int index = Mathf.Clamp(count - 1, 0, data.DiminishingCurve.Length - 1);
            return data.DiminishingCurve[index];
        }

        public float GetNextDiminishingMultiplier(CampActionType type)
        {
            var data = _database.GetAction(type);
            if (data?.DiminishingCurve == null || data.DiminishingCurve.Length == 0) return 1f;
            int count = GetRepeatCount(type);
            int index = Mathf.Clamp(count, 0, data.DiminishingCurve.Length - 1);
            return data.DiminishingCurve[index];
        }

        private void IncrementRepeat(CampActionType type)
        {
            _repeatCounts.TryGetValue(type, out int current);
            _repeatCounts[type] = current + 1;
        }

    }
}
