using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Lifecycle;

namespace Internal.Scripts.Npc.Behavior.Phases
{
    public sealed class ForagePhase : INpcDayPhase
    {
        private readonly NpcSimulationSettings _settings;

        public ForagePhase(NpcSimulationSettings settings)
        {
            _settings = settings;
        }

        public void Execute(NpcDayContext ctx, int i)
        {
            var eco = ctx.Economies[i];

            if (string.IsNullOrEmpty(ctx.DestinationNodeIds[i]))
                return;

            int supplies = InventoryStateMutator.GetItemCount(eco.Inventory, SuppliesItemId.Value);
            bool canForage = supplies < _settings.ForageThreshold
                && (ctx.CurrentDay - eco.LastForageDay >= _settings.ForageCooldownDays);

            if (canForage)
            {
                InventoryStateMutator.AddItems(eco.Inventory, SuppliesItemId.Value, _settings.ForageAmount);
                eco.LastForageDay = ctx.CurrentDay;
                ctx.ForagedIndices.Add(i);
                if (ctx.SuppliesSnapshot != null && i < ctx.SuppliesSnapshot.Length)
                    ctx.SuppliesSnapshot[i] = supplies + _settings.ForageAmount;
            }
            else
            {
                if (ctx.SuppliesSnapshot != null && i < ctx.SuppliesSnapshot.Length)
                    ctx.SuppliesSnapshot[i] = supplies;
            }
        }
    }
}
