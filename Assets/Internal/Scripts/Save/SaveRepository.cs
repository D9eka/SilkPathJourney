using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Save;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.Quests.Save;

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
        private bool _activeSlotIsAutoSave;

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

        public void MarkAsManual()
        {
            _activeSlotIsAutoSave = false;
        }

        public void Save()
        {
            if (_data == null)
                return;

            string currentSlot = _activeSaveSlot.SlotId;
            _loadedSlotId = currentSlot;

            var metadata = BuildMetadata(currentSlot, _activeSlotIsAutoSave);
            _saveService.Save(currentSlot, _data, metadata);
        }

        public void SaveToSlot(string slotId, bool isAutoSave)
        {
            if (_data == null)
                return;

            var metadata = BuildMetadata(slotId, isAutoSave);
            _saveService.Save(slotId, _data, metadata);
        }

        private SaveMetadata BuildMetadata(string slotId, bool isAutoSave)
        {
            var resources = _data.Economy?.PlayerResources;
            var carts = resources?.Carts;
            int cartCount = carts?.Count ?? 0;
            int otherDur = 0;
            int otherDurMax = 0;
            if (cartCount > 0)
            {
                float totalDur = 0f;
                float totalMax = 0f;
                foreach (var c in carts)
                {
                    totalDur += c.Durability;
                    totalMax += c.MaxDurability;
                }
                otherDur = (int)(totalDur / cartCount);
                otherDurMax = (int)(totalMax / cartCount);
            }

            return new SaveMetadata
            {
                SlotId = slotId,
                LastSavedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                CurrentDay = _data.Player?.CurrentDay ?? 1,
                DisplayName = $"День {_data.Player?.CurrentDay ?? 1}",
                CurrentNodeId = _data.Player?.CurrentNodeId ?? "",
                Money = resources?.Money ?? 0,
                Food = InventoryStateMutator.GetItemCount(_data.Economy?.PlayerInventory, SuppliesItemId.Value),
                CartDurability = (int)(resources?.PlayerCart?.Durability ?? 0),
                Danger = (int)(resources?.AccumulatedDanger ?? 0),
                PartnerCount = cartCount,
                IsAutoSave = isAutoSave,
                FoodMax = 0,
                CartDurabilityMax = (int)(resources?.PlayerCart?.MaxDurability ?? 100),
                DangerMax = (int)_balanceConfig.MaxDanger,
                OtherCartsDurability = otherDur,
                OtherCartsDurabilityMax = otherDurMax
            };
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

            var allSaves = _saveService.GetAllSaves();
            var existing = allSaves.Find(s => s.SlotId == currentSlot);
            _activeSlotIsAutoSave = existing?.IsAutoSave ?? false;
        }

        private static void Normalize(SaveData data)
        {
            data.Economy ??= new EconomySaveData();
            data.Player ??= new PlayerSaveData();
            data.Camera ??= new CameraSaveData();
            data.Roads ??= new RoadSaveData();
            data.Npcs ??= new NpcSaveData();
            data.Economy.PlayerInventory ??= new InventoryState();
            data.Quests ??= new QuestSaveData();
            data.Economy.CityInventories ??= new List<CityInventoryState>();

            if (!data.Economy.IsInitialized && data.Economy.CityInventories.Count > 0)
                data.Economy.IsInitialized = true;
        }
    }
}
