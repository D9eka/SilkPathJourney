using Internal.Scripts.Economy.Guild;
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

            GuildContract contract = ctx.Economy.ActiveContract.Value;

            if (contract.ContractType == GuildContractType.Cargo && !string.IsNullOrEmpty(contract.CargoItemId))
            {
                string cityId = ctx.City.Id;
                string cargoItemId = contract.CargoItemId;
                int cargoAmount = contract.CargoAmount;
                _inventoryRepo.UpdateCityInventoryState(cityId,
                    s => InventoryStateMutator.AddItems(s.GuildInventory, cargoItemId, cargoAmount));
            }

            int reward = contract.RewardMoney;
            ctx.Economy.Money += reward;
            ctx.Economy.ActiveContract = null;
            ctx.Traded = true;
            ctx.ContractReward = reward;

            Debug.Log($"[NpcContract] {ctx.Economy.Name} delivered {contract.ContractType} contract to {ctx.City.Id}, reward={reward}");
        }
    }
}
