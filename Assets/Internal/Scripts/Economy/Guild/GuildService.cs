using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Road.Path;
using Internal.Scripts.Save;
using UnityEngine;

namespace Internal.Scripts.Economy.Guild
{
    public sealed class GuildService
    {
        private readonly SaveRepository _saveRepository;
        private readonly PlayerResourceRepository _playerResources;
        private readonly InventoryRepository _inventoryRepository;
        private readonly EconomyDatabase _economyDatabase;
        private readonly DayTracker _dayTracker;
        private readonly IRoadPathFinder _pathFinder;
        private readonly GuildSettings _guildSettings;
        private readonly ItemCatalog _itemCatalog;

        public GuildService(
            SaveRepository saveRepository,
            PlayerResourceRepository playerResources,
            InventoryRepository inventoryRepository,
            EconomyDatabase economyDatabase,
            DayTracker dayTracker,
            IRoadPathFinder pathFinder,
            GuildSettings guildSettings,
            ItemCatalog itemCatalog)
        {
            _saveRepository = saveRepository;
            _playerResources = playerResources;
            _inventoryRepository = inventoryRepository;
            _economyDatabase = economyDatabase;
            _dayTracker = dayTracker;
            _pathFinder = pathFinder;
            _guildSettings = guildSettings;
            _itemCatalog = itemCatalog;
        }

        public bool IsMember => _saveRepository.Data.Economy.Guild.IsMember;
        public bool HasActiveContract => _saveRepository.Data.Economy.Guild.HasActiveContract;

        public bool CanJoin(string cityId)
        {
            return CityHasGuild(cityId) && _playerResources.Current.Money >= _guildSettings.JoinCost;
        }

        public void Join(string cityId)
        {
            if (!CanJoin(cityId))
                return;

            _playerResources.UpdateResources(s => s.Money -= _guildSettings.JoinCost);

            GuildSaveState guild = _saveRepository.Data.Economy.Guild;
            guild.IsMember = true;
            guild.JoinDay = _dayTracker.CurrentDay;
            _saveRepository.Save();
        }

        public bool CanTakeCredit(string cityId)
        {
            if (!IsMember || _saveRepository.Data.Economy.Guild.CreditAmount != 0)
                return false;

            if (!CityHasGuild(cityId))
                return false;

            CityInventoryState cityInv = _inventoryRepository.GetCityInventory(cityId);
            return cityInv != null && cityInv.GuildMoney >= _guildSettings.PlayerCreditAmount;
        }

        public void TakeCredit(string cityId)
        {
            if (!CanTakeCredit(cityId))
                return;

            _inventoryRepository.UpdateCityInventoryState(cityId, s => s.GuildMoney -= _guildSettings.PlayerCreditAmount);
            _playerResources.UpdateResources(s => s.Money += _guildSettings.PlayerCreditAmount);

            GuildSaveState guild = _saveRepository.Data.Economy.Guild;
            guild.CreditAmount = _guildSettings.PlayerCreditRepayment;
            guild.CreditTakenDay = _dayTracker.CurrentDay;
            _saveRepository.Save();
        }

        public bool CanRepayCredit(string cityId)
        {
            if (_saveRepository.Data.Economy.Guild.CreditAmount <= 0)
                return false;

            return CityHasGuild(cityId) && _playerResources.Current.Money >= _saveRepository.Data.Economy.Guild.CreditAmount;
        }

        public void RepayCredit(string cityId)
        {
            if (!CanRepayCredit(cityId))
                return;

            int amount = _saveRepository.Data.Economy.Guild.CreditAmount;
            _playerResources.UpdateResources(s => s.Money -= amount);
            _inventoryRepository.UpdateCityInventoryState(cityId, s => s.GuildMoney += amount);

            GuildSaveState guild = _saveRepository.Data.Economy.Guild;
            guild.CreditAmount = 0;
            _saveRepository.Save();
        }

        public void CheckCreditOverdue()
        {
            GuildSaveState guild = _saveRepository.Data.Economy.Guild;
            if (guild.CreditAmount <= 0)
                return;

            int elapsed = _dayTracker.CurrentDay - guild.CreditTakenDay;

            if (elapsed > _guildSettings.CreditExpelDays)
            {
                guild.IsMember = false;
                _saveRepository.Save();
                return;
            }

            if (elapsed > _guildSettings.CreditOverdueDays)
            {
                _playerResources.UpdateResources(s => s.Reputation += _guildSettings.ReputationOverduePenalty);
            }
        }

        public List<GuildContract> GetAvailableContracts(string cityId, float caravanSpeed)
        {
            CityData originCity = _economyDatabase.Cities.Find(c => c.Id == cityId);
            if (originCity == null)
                return new List<GuildContract>();

            CityInventoryState originInv = _inventoryRepository.GetCityInventory(cityId);
            if (originInv == null)
                return new List<GuildContract>();

            List<GuildContract> result = new List<GuildContract>();

            foreach (CityData city in _economyDatabase.Cities)
            {
                if (city == null || city.Id == cityId)
                    continue;

                if (!city.HasBuilding(BuildingId.Guild))
                    continue;

                RoadPath path = _pathFinder.FindPath(originCity.NodeId, city.NodeId);
                if (!path.IsValid)
                    continue;

                int distanceDays = path.EstimateDays(caravanSpeed);
                if (distanceDays <= 0)
                    continue;

                int reward = Mathf.RoundToInt(distanceDays * _guildSettings.ContractRewardPerDay) + _guildSettings.ContractBaseReward;
                if (originInv.GuildMoney < reward)
                    continue;

                result.Add(new GuildContract
                {
                    Id = $"{cityId}_{city.Id}_{_dayTracker.CurrentDay}",
                    OriginCityId = cityId,
                    TargetCityId = city.Id,
                    RewardMoney = reward,
                    ExpirationDay = _dayTracker.CurrentDay + Mathf.RoundToInt(distanceDays * _guildSettings.ContractExpirationMult),
                    GeneratedDay = _dayTracker.CurrentDay,
                    ContractType = GuildContractType.Courier
                });
            }

            if (originInv.GuildInventory?.Items != null)
            {
                foreach (ItemStackState stack in originInv.GuildInventory.Items)
                {
                    if (stack == null || stack.Count <= 0) continue;

                    float weightKg = _itemCatalog.GetItemWeight(stack.ItemId);
                    if (weightKg <= 0f) continue;

                    int cargoAmount = Mathf.Min(stack.Count, Mathf.FloorToInt(_guildSettings.CargoWeightBudget / weightKg));
                    if (cargoAmount < 1) continue;

                    foreach (CityData targetCity in _economyDatabase.Cities)
                    {
                        if (targetCity == null || targetCity.Id == cityId) continue;
                        if (!targetCity.HasBuilding(BuildingId.Guild)) continue;

                        CityInventoryState destInv = _inventoryRepository.GetCityInventory(targetCity.Id);
                        int destStock = InventoryStateMutator.GetItemCount(destInv?.GuildInventory, stack.ItemId);
                        if (destStock >= _guildSettings.CityOrderMaxStock) continue;

                        RoadPath path = _pathFinder.FindPath(originCity.NodeId, targetCity.NodeId);
                        if (!path.IsValid) continue;

                        int distanceDays = path.EstimateDays(caravanSpeed);
                        if (distanceDays <= 0) continue;

                        int basePrice = _itemCatalog.GetItemPrice(stack.ItemId);
                        float rewardFactor = _guildSettings.CargoRewardFactor;
                        if (destStock < _guildSettings.CityOrderMinStock)
                            rewardFactor *= _guildSettings.CargoContractBonusRewardFactor;

                        int reward = Mathf.RoundToInt(cargoAmount * basePrice * rewardFactor)
                                   + Mathf.RoundToInt(distanceDays * _guildSettings.ContractRewardPerDay)
                                   + _guildSettings.ContractBaseReward;

                        if (originInv.GuildMoney < reward) continue;

                        result.Add(new GuildContract
                        {
                            Id = $"{cityId}_{targetCity.Id}_{_dayTracker.CurrentDay}_cargo",
                            OriginCityId = cityId,
                            TargetCityId = targetCity.Id,
                            RewardMoney = reward,
                            ExpirationDay = _dayTracker.CurrentDay + Mathf.RoundToInt(distanceDays * _guildSettings.ContractExpirationMult),
                            GeneratedDay = _dayTracker.CurrentDay,
                            ContractType = GuildContractType.Cargo,
                            CargoItemId = stack.ItemId,
                            CargoAmount = cargoAmount
                        });

                        break;
                    }
                }
            }

            result.Sort((a, b) => b.RewardMoney.CompareTo(a.RewardMoney));

            if (result.Count > 3)
                result.RemoveRange(3, result.Count - 3);

            return result;
        }

        public void AcceptContract(GuildContract contract)
        {
            GuildSaveState guild = _saveRepository.Data.Economy.Guild;
            guild.ActiveContract = contract;
            guild.HasActiveContract = true;

            _inventoryRepository.UpdateCityInventoryState(contract.OriginCityId, s => s.GuildMoney -= contract.RewardMoney);

            if (contract.ContractType == GuildContractType.Cargo && !string.IsNullOrEmpty(contract.CargoItemId))
            {
                string cargoItem = contract.CargoItemId;
                int cargoAmount = contract.CargoAmount;
                _inventoryRepository.UpdateCityInventoryState(contract.OriginCityId,
                    s => InventoryStateMutator.RemoveItems(s.GuildInventory, cargoItem, cargoAmount));
                _inventoryRepository.UpdatePlayerInventory(
                    inv => InventoryStateMutator.AddItems(inv, cargoItem, cargoAmount));
            }

            _saveRepository.Save();
        }

        public bool TryCompleteContract(string cityId)
        {
            GuildSaveState guild = _saveRepository.Data.Economy.Guild;
            if (!guild.HasActiveContract)
                return false;

            GuildContract contract = guild.ActiveContract;
            if (contract.TargetCityId != cityId)
                return false;

            if (_dayTracker.CurrentDay > contract.ExpirationDay)
                return false;

            if (contract.ContractType == GuildContractType.Cargo && !string.IsNullOrEmpty(contract.CargoItemId))
            {
                int playerStock = InventoryStateMutator.GetItemCount(
                    _inventoryRepository.GetPlayerInventory(), contract.CargoItemId);
                if (playerStock < contract.CargoAmount)
                    return false;

                string cargoItem = contract.CargoItemId;
                int cargoAmount = contract.CargoAmount;
                _inventoryRepository.UpdatePlayerInventory(
                    inv => InventoryStateMutator.RemoveItems(inv, cargoItem, cargoAmount));
                _inventoryRepository.UpdateCityInventoryState(cityId,
                    s => InventoryStateMutator.AddItems(s.GuildInventory, cargoItem, cargoAmount));
            }

            _playerResources.UpdateResources(s => s.Money += contract.RewardMoney);

            guild.HasActiveContract = false;
            _saveRepository.Save();

            return true;
        }

        public float GetTariffMultiplier()
        {
            return IsMember ? (1f - _guildSettings.MemberTariffDiscount) : 1.0f;
        }

        public bool CityHasGuild(string cityId)
        {
            CityData city = _economyDatabase.Cities.Find(c => c.Id == cityId);
            return city != null && city.HasBuilding(BuildingId.Guild);
        }
    }
}
