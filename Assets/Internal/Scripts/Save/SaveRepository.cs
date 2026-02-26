using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Save.Models;

namespace Internal.Scripts.Save
{
    public sealed class SaveRepository
    {
        private readonly ISaveService _saveService;
        private readonly ActiveSaveSlot _activeSaveSlot;
        private readonly GameBalanceConfig _balanceConfig;
        private SaveData _data;
        private bool _isLoaded;
        private string _loadedSlotId;

        public SaveRepository(ISaveService saveService, ActiveSaveSlot activeSaveSlot, GameBalanceConfig balanceConfig)
        {
            _saveService = saveService;
            _activeSaveSlot = activeSaveSlot;
            _balanceConfig = balanceConfig;
        }

        public SaveData Data
        {
            get
            {
                EnsureLoaded();
                return _data;
            }
        }

        public void Save(bool isAutoSave = true)
        {
            if (_data == null)
                return;

            string currentSlot = _activeSaveSlot.SlotId;
            _loadedSlotId = currentSlot;

            var resources = _data.Economy?.PlayerResources;
            var metadata = new SaveMetadata
            {
                SlotId = currentSlot,
                LastSavedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                CurrentDay = _data.Player?.CurrentDay ?? 1,
                DisplayName = $"День {_data.Player?.CurrentDay ?? 1}",
                CurrentNodeId = _data.Player?.CurrentNodeId ?? "",
                Money = resources?.Money ?? 0,
                Food = (int)(resources?.Food ?? 0),
                CartDurability = (int)(resources?.PlayerCart?.Durability ?? 0),
                Danger = (int)(resources?.AccumulatedDanger ?? 0),
                PartnerCount = resources?.Carts?.Count ?? 0,
                IsAutoSave = isAutoSave,
                FoodMax = (int)_balanceConfig.MaxFood,
                CartDurabilityMax = (int)(resources?.PlayerCart?.MaxDurability ?? 100),
                DangerMax = (int)_balanceConfig.MaxDanger
            };

            _saveService.Save(currentSlot, _data, metadata);
        }

        private void EnsureLoaded()
        {
            string currentSlot = _activeSaveSlot.SlotId;
            if (_isLoaded && _loadedSlotId == currentSlot)
                return;

            _data = _saveService.Load(currentSlot) ?? new SaveData
            {
                Economy = new EconomySaveData(),
                Player = new PlayerSaveData()
            };

            Normalize(_data);
            _isLoaded = true;
            _loadedSlotId = currentSlot;
        }

        private static void Normalize(SaveData data)
        {
            data.Economy ??= new EconomySaveData();
            data.Player ??= new PlayerSaveData();
            data.Camera ??= new CameraSaveData();
            data.Roads ??= new RoadSaveData();
            data.Economy.PlayerInventory ??= new InventoryState();
            data.Economy.CityInventories ??= new List<CityInventoryState>();

            if (!data.Economy.IsInitialized && data.Economy.CityInventories.Count > 0)
                data.Economy.IsInitialized = true;
        }
    }
}
