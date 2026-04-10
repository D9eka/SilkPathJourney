using System;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using UnityEngine;

namespace Internal.Scripts.Npc.Trading
{
    public sealed class NpcGuildTradeService
    {
        private readonly EconomyDatabase _economyDatabase;
        private readonly InventoryRepository _inventoryRepository;
        private readonly ItemCatalog _itemCatalog;
        private readonly NpcSimulationSettings _settings;
        private readonly GuildSettings _guildSettings;
        private readonly NpcSupplyPlanner _supplyPlanner;
        private readonly DayTracker _dayTracker;

        public NpcGuildTradeService(
            EconomyDatabase economyDatabase,
            InventoryRepository inventoryRepository,
            ItemCatalog itemCatalog,
            NpcSimulationSettings settings,
            GuildSettings guildSettings,
            NpcSupplyPlanner supplyPlanner,
            DayTracker dayTracker)
        {
            _economyDatabase = economyDatabase;
            _inventoryRepository = inventoryRepository;
            _itemCatalog = itemCatalog;
            _settings = settings;
            _guildSettings = guildSettings;
            _supplyPlanner = supplyPlanner;
            _dayTracker = dayTracker;
        }

        public void TryTakeGuildContract(NpcEconomyState agent, string cityId, string nextDestNodeId,
            float speedMetersPerDay, Func<float> nextRandom = null)
        {
            if (agent.ActiveContract.HasValue) return;

            CityData city = null;
            var guildCities = new System.Collections.Generic.List<CityData>();
            foreach (CityData c in _economyDatabase.Cities)
            {
                if (c.Id == cityId) city = c;
                else if (c.HasBuilding(BuildingId.Guild)) guildCities.Add(c);
            }
            if (city == null || !city.HasBuilding(BuildingId.Guild)) return;
            if (guildCities.Count == 0) return;

            float roll = nextRandom != null ? nextRandom() : UnityEngine.Random.value;
            if (roll > _guildSettings.ContractAcceptChance) return;

            int targetIndex = nextRandom != null
                ? Mathf.Clamp((int)(nextRandom() * guildCities.Count), 0, guildCities.Count - 1)
                : UnityEngine.Random.Range(0, guildCities.Count);
            CityData target = guildCities[targetIndex];

            float estimatedDays = _supplyPlanner.EstimateTransportDays(city.NodeId, target.NodeId, speedMetersPerDay);
            if (estimatedDays <= 0f) estimatedDays = _guildSettings.ContractFallbackDays;

            int reward = Mathf.RoundToInt(estimatedDays * _guildSettings.ContractRewardPerDay + _guildSettings.ContractBaseReward);

            CityInventoryState cityInv = _inventoryRepository.GetCityInventory(cityId);
            if (cityInv == null || cityInv.GuildMoney < reward) return;

            float cargoRoll = nextRandom != null ? nextRandom() : UnityEngine.Random.value;
            if (cargoRoll <= _guildSettings.NpcCargoContractChance)
            {
                var availableStacks = cityInv.GuildInventory?.Items?.FindAll(s => s.Count > 0);
                if (availableStacks != null && availableStacks.Count > 0)
                {
                    float currentWeight = 0f;
                    foreach (var stack in agent.Inventory.Items)
                        currentWeight += _itemCatalog.GetItemWeight(stack.ItemId) * stack.Count;

                    float freeCapacity = agent.CapacityKg - currentWeight;

                    if (freeCapacity >= _guildSettings.CargoWeightBudget)
                    {
                        int stackIndex = nextRandom != null
                            ? Mathf.Clamp((int)(nextRandom() * availableStacks.Count), 0, availableStacks.Count - 1)
                            : UnityEngine.Random.Range(0, availableStacks.Count);

                        var chosenStack = availableStacks[stackIndex];
                        float weightKg = _itemCatalog.GetItemWeight(chosenStack.ItemId);
                        int cargoAmount = weightKg > 0f
                            ? Mathf.Min(chosenStack.Count, Mathf.FloorToInt(_guildSettings.CargoWeightBudget / weightKg))
                            : chosenStack.Count;

                        if (cargoAmount > 0)
                        {
                            int basePrice = _itemCatalog.GetItemPrice(chosenStack.ItemId);
                            int totalReward = reward + Mathf.RoundToInt(cargoAmount * basePrice * _guildSettings.CargoRewardFactor);

                            if (cityInv.GuildMoney < totalReward) totalReward = reward;

                            string cargoItemId = chosenStack.ItemId;
                            _inventoryRepository.UpdateCityInventoryState(cityId, s =>
                            {
                                s.GuildMoney -= totalReward;
                                InventoryStateMutator.RemoveItems(s.GuildInventory, cargoItemId, cargoAmount);
                            });

                            agent.ActiveContract = new GuildContract
                            {
                                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                                OriginCityId = cityId,
                                TargetCityId = target.Id,
                                RewardMoney = totalReward,
                                GeneratedDay = _dayTracker.CurrentDay,
                                ExpirationDay = _dayTracker.CurrentDay + Mathf.RoundToInt(estimatedDays * _guildSettings.ContractExpirationMult),
                                ContractType = GuildContractType.Cargo,
                                CargoItemId = cargoItemId,
                                CargoAmount = cargoAmount
                            };

                            Debug.Log($"[NpcContract] {agent.Name} took cargo contract {cityId}→{target.Id}, item={cargoItemId} x{cargoAmount}, reward={totalReward}g");
                            return;
                        }
                    }
                }
            }

            _inventoryRepository.UpdateCityInventoryState(cityId, s => s.GuildMoney -= reward);

            agent.ActiveContract = new GuildContract
            {
                Id = Guid.NewGuid().ToString("N").Substring(0, 8),
                OriginCityId = cityId,
                TargetCityId = target.Id,
                RewardMoney = reward,
                GeneratedDay = _dayTracker.CurrentDay,
                ExpirationDay = _dayTracker.CurrentDay + Mathf.RoundToInt(estimatedDays * _guildSettings.ContractExpirationMult),
                ContractType = GuildContractType.Courier
            };

            Debug.Log($"[NpcContract] {agent.Name} took courier contract {cityId}→{target.Id} for {reward}g");
        }

        public void HandleDebtRepayment(NpcEconomyState agent, string cityId)
        {
            CityData city = _economyDatabase.Cities.Find(c => c.Id == cityId);
            if (city == null || !city.HasBuilding(BuildingId.Guild))
                return;

            if (!agent.InDebt || agent.Debt <= 0)
                return;

            int surplus = agent.Money - _settings.SurvivalMoneyThreshold;
            if (surplus <= 0)
                return;

            int repayment = Mathf.Min(
                Mathf.RoundToInt(surplus * _settings.DebtRepaymentFraction),
                Mathf.RoundToInt(agent.Debt));

            agent.Money -= repayment;
            agent.Debt -= repayment;
            _inventoryRepository.UpdateCityInventory(cityId, inv => inv.Money += repayment);

            if (agent.Debt <= 0)
            {
                agent.Debt = 0;
                agent.InDebt = false;
                Debug.Log($"[NpcTrader] {agent.Name} repaid debt in full");
            }
            else
            {
                Debug.Log($"[NpcTrader] {agent.Name} repaid {repayment}g, remaining debt: {agent.Debt:F0}g");
            }
        }

        public void HandleGuildCredit(NpcEconomyState agent, string cityId)
        {
            if (agent.Money >= _settings.CreditMoneyThreshold || agent.InDebt)
                return;

            CityData city = _economyDatabase.Cities.Find(c => c.Id == cityId);
            if (city == null || !city.HasBuilding(BuildingId.Guild))
                return;

            CityInventoryState cityInv = _inventoryRepository.GetCityInventory(cityId);
            if (cityInv == null || cityInv.GuildMoney < _settings.CreditAmount)
                return;

            int amount = _settings.CreditAmount;
            _inventoryRepository.UpdateCityInventoryState(cityId, s => s.GuildMoney -= amount);
            agent.Money += amount;
            agent.Debt = amount;
            agent.InDebt = true;
            Debug.Log($"[NpcTrader] {agent.Name} took guild credit of {amount}g");
        }

    }
}
