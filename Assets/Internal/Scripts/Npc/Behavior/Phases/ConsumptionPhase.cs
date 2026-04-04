using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Lifecycle;
using UnityEngine;

namespace Internal.Scripts.Npc.Behavior.Phases
{
    public sealed class ConsumptionPhase : INpcDayPhase
    {
        private readonly NpcSimulationSettings _settings;

        public ConsumptionPhase(NpcSimulationSettings settings)
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

            if (supplies <= 0)
                return;

            int toConsume = Mathf.Min(_settings.SuppliesPerDay, supplies);
            InventoryStateMutator.RemoveItems(ctx.Economies[i].Inventory, SuppliesItemId.Value, toConsume);
        }
    }
}
