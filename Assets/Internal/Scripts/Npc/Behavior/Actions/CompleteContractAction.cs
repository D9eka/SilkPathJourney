using Internal.Scripts.Inventory;
using UnityEngine;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class CompleteContractAction : ICityVisitAction
    {
        private readonly InventoryRepository _inventoryRepo;

        public CompleteContractAction(InventoryRepository inventoryRepo)
        {
            _inventoryRepo = inventoryRepo;
        }

        public void Execute(NpcCityVisitContext ctx)
        {
            if (!ctx.Economy.ActiveContract.HasValue || ctx.Economy.ActiveContract.Value.TargetCityId != ctx.City.Id)
                return;

            int reward = ctx.Economy.ActiveContract.Value.RewardMoney;
            ctx.Economy.Money += reward;
            ctx.Economy.ActiveContract = null;
            ctx.Traded = true;
            ctx.ContractReward = reward;

            Debug.Log($"[NpcContract] {ctx.Economy.Name} delivered contract to {ctx.City.Id}, reward={reward}");
        }
    }
}
