using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Save;
using Zenject;

namespace Internal.Scripts.Economy.Inventory
{
    public sealed class InventoryRepository : IInitializable
    {
        private readonly SaveRepository _saveRepository;

        private EconomySaveData _saveData;
        private bool _isLoaded;

        public event Action PlayerInventoryChanged;
        public event Action<string> CityInventoryChanged;

        public InventoryRepository(
            SaveRepository saveRepository)
        {
            _saveRepository = saveRepository;
        }

        public void Initialize()
        {
            EnsureLoaded();
        }

        public InventoryState GetPlayerInventory()
        {
            EnsureLoaded();
            return _saveData.PlayerInventory;
        }

        public CityInventoryState GetCityInventory(string cityId)
        {
            EnsureLoaded();
            return _saveData.CityInventories.Find(c => c.CityId == cityId);
        }

        public bool UpdatePlayerInventory(Action<InventoryState> mutator)
        {
            if (mutator == null)
                return false;

            EnsureLoaded();
            mutator(_saveData.PlayerInventory);
            _saveRepository.Save();
            PlayerInventoryChanged?.Invoke();
            return true;
        }

        public bool UpdateCityInventory(string cityId, Action<InventoryState> mutator)
        {
            if (mutator == null || string.IsNullOrWhiteSpace(cityId))
                return false;

            EnsureLoaded();
            CityInventoryState cityState = _saveData.CityInventories.Find(c => c.CityId == cityId);
            if (cityState == null)
                return false;

            mutator(cityState.Inventory);
            _saveRepository.Save();
            CityInventoryChanged?.Invoke(cityId);
            return true;
        }

        public void NotifyPlayerInventoryChanged()
        {
            EnsureLoaded();
            _saveRepository.Save();
            PlayerInventoryChanged?.Invoke();
        }

        public void NotifyCityInventoryChanged(string cityId)
        {
            if (string.IsNullOrWhiteSpace(cityId))
                return;

            EnsureLoaded();
            _saveRepository.Save();
            CityInventoryChanged?.Invoke(cityId);
        }

        public void Save()
        {
            EnsureLoaded();
            _saveRepository.Save();
        }

        private void EnsureLoaded()
        {
            if (_isLoaded)
                return;

            SaveData root = _saveRepository.Data;
            _saveData = root.Economy;
            if (_saveData == null)
            {
                UnityEngine.Debug.LogWarning("[SPJ] Economy save is missing. Ensure SaveBootstrapper ran.");
                _saveData = new EconomySaveData();
            }

            if (_saveData.PlayerInventory == null)
                _saveData.PlayerInventory = new InventoryState();

            if (_saveData.CityInventories == null)
                _saveData.CityInventories = new List<CityInventoryState>();

            _isLoaded = true;
        }
    }
}
