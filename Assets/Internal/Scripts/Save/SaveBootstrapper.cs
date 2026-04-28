using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Npc.Save;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Background;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.Road.State;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Save
{
    public sealed class SaveBootstrapper : IInitializable
    {
        private readonly SaveRepository _saveRepository;
        private readonly EconomySaveBuilder _economySaveBuilder;
        private readonly PlayerConfig _playerConfig;
        private readonly RoadUnlockService _roadUnlockService;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly CaravanDatabase _caravanDatabase;
        private readonly BackgroundDatabase _backgroundDatabase;
        private readonly ActiveSaveSlot _activeSaveSlot;
        private readonly EconomyDatabase _economyDatabase;

        public SaveBootstrapper(
            SaveRepository saveRepository,
            EconomySaveBuilder economySaveBuilder,
            PlayerConfig playerConfig,
            RoadUnlockService roadUnlockService,
            GameBalanceConfig balanceConfig,
            CaravanDatabase caravanDatabase,
            BackgroundDatabase backgroundDatabase,
            ActiveSaveSlot activeSaveSlot,
            EconomyDatabase economyDatabase)
        {
            _saveRepository = saveRepository;
            _economySaveBuilder = economySaveBuilder;
            _playerConfig = playerConfig;
            _roadUnlockService = roadUnlockService;
            _balanceConfig = balanceConfig;
            _caravanDatabase = caravanDatabase;
            _backgroundDatabase = backgroundDatabase;
            _activeSaveSlot = activeSaveSlot;
            _economyDatabase = economyDatabase;
        }

        public void Initialize()
        {
            SaveData data = _saveRepository.Data;
            bool changed = false;

            if (data.Version < 2)
                changed |= MigrateToV2(data);

            if (data.Version < 3)
                changed |= MigrateToV3(data);

            if (data.Version < 4)
                changed |= MigrateToV4(data);

            if (data.Version < 5)
                changed |= MigrateToV5(data);

            if (data.Version < 6)
                changed |= MigrateToV6(data);

            changed |= SyncActiveSaveSlotIds(data);

            BackgroundData selectedBackground = _backgroundDatabase?.GetById(_activeSaveSlot?.SelectedBackgroundId);

            if (data.Economy == null || !data.Economy.IsInitialized)
            {
                CartClassData cartClass = _caravanDatabase?.GetCartClassById(_activeSaveSlot?.SelectedCartClassId);
                data.Economy = _economySaveBuilder.Build(selectedBackground, cartClass);
                ApplyBackgroundToSkillsAndLanguages(data, selectedBackground);
                changed = true;
            }

            if (data.Player == null || string.IsNullOrWhiteSpace(data.Player.CurrentNodeId))
            {
                if (data.Player == null)
                    data.Player = new PlayerSaveData();

                data.Player.CurrentNodeId = ResolveStartNodeId(selectedBackground);
                data.Player.DestinationNodeId = string.Empty;
                data.Player.State = PlayerState.Idle;
                changed = true;
            }

            if (data.Roads != null)
                _roadUnlockService.LoadState(data.Roads.UnlockedRoadIds);

            if (changed)
                _saveRepository.Save();
        }

        private bool MigrateToV2(SaveData data)
        {
            float secondsPerDay = _balanceConfig != null
                ? Mathf.Max(0f, _balanceConfig.SecondsPerDay)
                : 0f;

            if (secondsPerDay > 0f)
            {
                PlayerResourceState resources = data.Economy?.PlayerResources;
                if (resources?.PlayerCart != null)
                {
                    resources.PlayerCart.Speed *= secondsPerDay;
                }

                if (resources?.Carts != null)
                {
                    foreach (CartState cart in resources.Carts)
                        cart.Speed *= secondsPerDay;
                }
            }

            data.Version = 2;
            return true;
        }

        private bool MigrateToV3(SaveData data)
        {
            data.Npcs ??= new NpcSaveData();
            data.Version = 3;
            return true;
        }

        private bool MigrateToV4(SaveData data)
        {
            data.Skills ??= new PlayerSkillState
            {
                Skills = new List<SkillEntry>
                {
                    new() { Type = SkillType.Survival, Value = 10 },
                    new() { Type = SkillType.Charisma, Value = 10 },
                    new() { Type = SkillType.Trade, Value = 20 }
                }
            };
            data.Version = 4;
            return true;
        }

        private bool MigrateToV5(SaveData data)
        {
            if (data.Languages == null || data.Languages.Languages.Count == 0)
            {
                data.Languages = new PlayerLanguageState
                {
                    Languages = new List<LanguageEntry>
                    {
                        new() { Type = LanguageType.Arabic, Proficiency = LanguageProficiency.Native }
                    }
                };
            }
            data.Version = 5;
            return true;
        }

        private bool MigrateToV6(SaveData data)
        {
            PlayerResourceState resources = data.Economy?.PlayerResources;
            if (resources == null)
            {
                data.Version = 6;
                return true;
            }

#pragma warning disable CS0618
            if (resources.PlayerCart != null && resources.PlayerCart.AnimalCount == 0
                && resources.PlayerCart.FoodConsumptionPerDay > 0f)
            {
                CartClassData classData = _caravanDatabase?.GetCartClassById(resources.CartClassId);
                if (classData != null)
                    resources.PlayerCart.AnimalCount = classData.AnimalCount;
            }

            if (resources.Carts != null)
            {
                foreach (CartState cart in resources.Carts)
                {
                    if (cart.AnimalCount != 0 || !(cart.FoodConsumptionPerDay > 0f))
                        continue;

                    if (string.IsNullOrEmpty(cart.TypeId))
                        continue;

                    ExtraCartData extraCart = _caravanDatabase?.ExtraCarts?.Find(e => e.Id == cart.TypeId);
                    if (extraCart != null)
                        cart.AnimalCount = extraCart.SuppliesPerDay;
                }
            }
#pragma warning restore CS0618

            data.Version = 6;
            return true;
        }

        private bool SyncActiveSaveSlotIds(SaveData data)
        {
            if (string.IsNullOrEmpty(data.SelectedBackgroundId) &&
                !string.IsNullOrEmpty(_activeSaveSlot.SelectedBackgroundId))
            {
                data.SelectedBackgroundId = _activeSaveSlot.SelectedBackgroundId;
                data.SelectedCartClassId = _activeSaveSlot.SelectedCartClassId;
                return true;
            }

            if (!string.IsNullOrEmpty(data.SelectedBackgroundId) &&
                string.IsNullOrEmpty(_activeSaveSlot.SelectedBackgroundId))
            {
                _activeSaveSlot.Restore(data.SelectedBackgroundId, data.SelectedCartClassId);
            }

            return false;
        }

        private static void ApplyBackgroundToSkillsAndLanguages(SaveData data, BackgroundData background)
        {
            if (background == null)
                return;

            data.Skills = new PlayerSkillState
            {
                Skills = new List<SkillEntry>
                {
                    new() { Type = SkillType.Trade, Value = background.TradeSkill },
                    new() { Type = SkillType.Charisma, Value = background.CharismaSkill },
                    new() { Type = SkillType.Survival, Value = background.SurvivalSkill }
                }
            };

            var languageEntries = new List<LanguageEntry>();
            foreach (LanguageStartEntry entry in background.StartingLanguages)
            {
                languageEntries.Add(new LanguageEntry
                {
                    Type = entry.LanguageType,
                    Proficiency = (LanguageProficiency)entry.Level
                });
            }
            data.Languages = new PlayerLanguageState { Languages = languageEntries };
        }

        private string ResolveStartNodeId(BackgroundData background = null)
        {
            if (background != null && !string.IsNullOrEmpty(background.StartingCityId) && _economyDatabase != null)
            {
                string cityId = background.StartingCityId;
                var city = _economyDatabase.Cities?.Find(c =>
                    string.Equals(c.Id, cityId, System.StringComparison.OrdinalIgnoreCase));
                if (city != null && !string.IsNullOrWhiteSpace(city.NodeId))
                    return city.NodeId;
            }

            if (_playerConfig != null && !string.IsNullOrWhiteSpace(_playerConfig.StartNodeId))
                return _playerConfig.StartNodeId;

            return "N_Quanzhou";
        }
    }
}
