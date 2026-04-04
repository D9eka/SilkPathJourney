using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Lifecycle;
using UnityEngine;

namespace Internal.Scripts.Npc.Behavior.Phases
{
    public sealed class StarvationPhase : INpcDayPhase
    {
        private readonly NpcSimulationSettings _settings;

        public StarvationPhase(NpcSimulationSettings settings)
        {
            _settings = settings;
        }

        public void Execute(NpcDayContext ctx, int i)
        {
            if (string.IsNullOrEmpty(ctx.DestinationNodeIds[i]))
                return;

            int supplies = ctx.SuppliesSnapshot != null && i < ctx.SuppliesSnapshot.Length
                ? ctx.SuppliesSnapshot[i]
                : InventoryStateMutator.GetItemCount(ctx.Economies[i].Inventory, SuppliesItemId.Value);

            if (supplies > 0)
                return;

            float daysToCity = ctx.EstimateDaysToCity != null ? ctx.EstimateDaysToCity(i) : 1f;
            float dailyDeath = daysToCity > 0f
                ? 1f - Mathf.Pow(_settings.StarvationSurvivalChance, 1f / daysToCity)
                : 1f;

            float roll = ctx.NextRandom != null ? (float)ctx.NextRandom() : UnityEngine.Random.value;
            if (roll < dailyDeath)
                ctx.DeadIndices.Add(i);
        }
    }
}
