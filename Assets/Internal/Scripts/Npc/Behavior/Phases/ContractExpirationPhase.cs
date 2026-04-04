using Internal.Scripts.Inventory;

namespace Internal.Scripts.Npc.Behavior.Phases
{
    public sealed class ContractExpirationPhase : INpcDayPhase
    {
        private readonly InventoryRepository _inventoryRepo;

        public ContractExpirationPhase(InventoryRepository inventoryRepo)
        {
            _inventoryRepo = inventoryRepo;
        }

        public void Execute(NpcDayContext ctx, int i)
        {
            var eco = ctx.Economies[i];
            if (!eco.ActiveContract.HasValue || ctx.CurrentDay <= eco.ActiveContract.Value.ExpirationDay)
                return;

            string origin = eco.ActiveContract.Value.OriginCityId;
            int reward = eco.ActiveContract.Value.RewardMoney;
            _inventoryRepo.UpdateCityInventoryState(origin, s => s.GuildMoney += reward);
            eco.ActiveContract = null;
        }
    }
}
