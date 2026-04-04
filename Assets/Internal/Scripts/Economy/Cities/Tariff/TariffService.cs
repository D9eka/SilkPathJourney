using Internal.Scripts.Config;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.Smuggling;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Trading;
using UnityEngine;

namespace Internal.Scripts.Economy.Cities.Tariff
{
    public sealed class TariffService
    {
        private readonly EconomyDatabase _economyDatabase;
        private readonly PlayerResourceRepository _playerResources;
        private readonly ItemCatalog _itemCatalog;
        private readonly InventoryRepository _inventoryRepository;
        private readonly GuildService _guildService;
        private readonly GuildSettings _guildSettings;
        private readonly GameBalanceConfig _balanceConfig;

        public TariffService(
            EconomyDatabase economyDatabase,
            PlayerResourceRepository playerResources,
            ItemCatalog itemCatalog,
            InventoryRepository inventoryRepository,
            GuildService guildService,
            GuildSettings guildSettings,
            GameBalanceConfig balanceConfig)
        {
            _economyDatabase = economyDatabase;
            _playerResources = playerResources;
            _itemCatalog = itemCatalog;
            _inventoryRepository = inventoryRepository;
            _guildService = guildService;
            _guildSettings = guildSettings;
            _balanceConfig = balanceConfig;
        }

        public int EstimateTariff(InventoryState inventory, CityData city, int reputation)
        {
            if (inventory?.Items == null || inventory.Items.Count == 0)
                return 0;

            CityTypeData cityType = _economyDatabase.GetCityType(city.Type);
            if (cityType == null)
                return 0;

            int totalValue = 0;
            foreach (ItemStackState stack in inventory.Items)
            {
                if (stack == null || stack.Count <= 0)
                    continue;

                int basePrice = _itemCatalog.GetItemPrice(stack.ItemId);
                totalValue += stack.Count * basePrice;
            }

            float reputationModifier = GetReputationModifier(reputation);
            return (int)(totalValue * cityType.BaseTariffPct / 100f * reputationModifier);
        }

        public TariffResult ChargePlayerTariff(CityData city)
        {
            InventoryState inventory = _inventoryRepository.GetPlayerInventory();
            int reputation = _playerResources.Current.Reputation;
            int tariff = EstimateTariff(inventory, city, reputation);
            tariff = (int)(tariff * _guildService.GetTariffMultiplier());

            if (tariff <= 0)
                return TariffResult.NoTariff;

            if (_playerResources.Current.Money < tariff)
                return TariffResult.Insufficient(tariff);

            _playerResources.UpdateResources(s => s.Money -= tariff);

            bool hasGuild = _guildService.CityHasGuild(city.Id);
            int guildShare = hasGuild ? Mathf.RoundToInt(tariff * _guildSettings.GuildTariffShare) : 0;
            _inventoryRepository.UpdateCityInventoryState(city.Id, s =>
            {
                s.Inventory.Money += tariff - guildShare;
                s.GuildMoney += guildShare;
            });

            return TariffResult.Charged(tariff);
        }

        public int ChargeNpcTariff(NpcEconomyState agent, CityData city)
        {
            int tariff = EstimateTariff(agent.Inventory, city, 50);
            int charged = tariff > agent.Money ? agent.Money : tariff;
            agent.Money -= charged;

            bool hasGuild = _guildService.CityHasGuild(city.Id);
            int guildShare = hasGuild ? Mathf.RoundToInt(charged * _guildSettings.GuildTariffShare) : 0;
            _inventoryRepository.UpdateCityInventoryState(city.Id, s =>
            {
                s.Inventory.Money += charged - guildShare;
                s.GuildMoney += guildShare;
            });

            return charged;
        }

        private float GetReputationModifier(int reputation) =>
            SmugglingModifierCalculator.EvaluateRules(
                _balanceConfig.TariffReputationRules, reputation,
                _balanceConfig.TariffDefaultReputationModifier);
    }
}
