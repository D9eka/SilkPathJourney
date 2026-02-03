using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Save;
using R3;
using Zenject;

namespace Internal.Scripts.Inventory
{
    public sealed class InventoryRepository : IInitializable
    {
        private readonly SaveRepository _saveRepository;

        private EconomySaveData _saveData;
        private bool _isLoaded;
        private readonly ReactiveProperty<InventoryState> _playerInventoryStream = new(new InventoryState());
        private readonly Dictionary<string, ReactiveProperty<InventoryState>> _cityInventoryStreams = new();

        public InventoryRepository(
            SaveRepository saveRepository)
        {
            _saveRepository = saveRepository;
        }

        public void Initialize()
        {
            EnsureLoaded();
        }

        public Observable<InventoryState> PlayerInventoryStream => _playerInventoryStream;

        public Observable<InventoryState> ObserveCityInventory(string cityId)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(cityId))
                return new ReactiveProperty<InventoryState>(new InventoryState());

            if (!_cityInventoryStreams.TryGetValue(cityId, out ReactiveProperty<InventoryState> stream))
            {
                InventoryState inventory = GetCityInventory(cityId)?.Inventory ?? new InventoryState();
                stream = new ReactiveProperty<InventoryState>(CloneInventory(inventory));
                _cityInventoryStreams[cityId] = stream;
            }

            return stream;
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
            UpdatePlayerStream();
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
            UpdateCityStream(cityId, cityState.Inventory);
            return true;
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

            UpdatePlayerStream();

            _isLoaded = true;
        }

        private void UpdatePlayerStream()
        {
            _playerInventoryStream.Value = CloneInventory(_saveData.PlayerInventory);
        }

        private void UpdateCityStream(string cityId, InventoryState inventory)
        {
            if (string.IsNullOrWhiteSpace(cityId))
                return;

            if (_cityInventoryStreams.TryGetValue(cityId, out ReactiveProperty<InventoryState> stream))
                stream.Value = CloneInventory(inventory);
        }

        private static InventoryState CloneInventory(InventoryState source)
        {
            if (source == null)
                return new InventoryState();

            InventoryState clone = new InventoryState
            {
                Money = source.Money,
                MaxWeightKg = source.MaxWeightKg,
                Items = new List<ItemStackState>()
            };

            if (source.Items == null)
                return clone;

            foreach (ItemStackState stack in source.Items)
            {
                if (stack == null)
                    continue;

                clone.Items.Add(new ItemStackState
                {
                    ItemId = stack.ItemId,
                    Count = stack.Count
                });
            }

            return clone;
        }
    }
}
