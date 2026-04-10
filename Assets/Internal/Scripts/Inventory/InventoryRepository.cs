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
        private readonly ReactiveProperty<InventoryState> _hiddenCompartmentStream = new(new InventoryState());
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
        public Observable<InventoryState> HiddenCompartmentStream => _hiddenCompartmentStream;

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

        public InventoryState GetHiddenCompartment()
        {
            EnsureLoaded();
            return _saveData.HiddenCompartment;
        }

        public bool UpdateHiddenCompartment(Action<InventoryState> mutator)
        {
            if (mutator == null)
                return false;

            EnsureLoaded();
            mutator(_saveData.HiddenCompartment);
            _saveRepository.Save();
            _hiddenCompartmentStream.Value = CloneInventory(_saveData.HiddenCompartment);
            return true;
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

        public bool UpdateCityInventoryState(string cityId, Action<CityInventoryState> mutator)
        {
            if (mutator == null || string.IsNullOrWhiteSpace(cityId))
                return false;

            EnsureLoaded();
            CityInventoryState cityState = _saveData.CityInventories.Find(c => c.CityId == cityId);
            if (cityState == null)
                return false;

            mutator(cityState);
            _saveRepository.Save();
            UpdateCityStream(cityId, cityState.Inventory);
            return true;
        }

        public List<CityInventoryState> GetAllCityInventories()
        {
            EnsureLoaded();
            return _saveData.CityInventories;
        }

        public void UpdateAllCityInventories(Action<List<CityInventoryState>> mutator)
        {
            if (mutator == null)
                return;

            EnsureLoaded();
            mutator(_saveData.CityInventories);
            _saveRepository.Save();

            foreach (CityInventoryState cityState in _saveData.CityInventories)
            {
                if (cityState != null && !string.IsNullOrWhiteSpace(cityState.CityId))
                    UpdateCityStream(cityState.CityId, cityState.Inventory);
            }
        }

        public bool HasPlayerItem(string itemId, int minCount = 1)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(itemId) || _saveData.PlayerInventory?.Items == null)
                return false;

            var stack = _saveData.PlayerInventory.Items.Find(s => s.ItemId == itemId);
            return stack != null && stack.Count >= minCount;
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
            if (root?.Economy == null)
            {
                UnityEngine.Debug.LogWarning("[SPJ] Economy save is missing. Ensure SaveBootstrapper ran.");
                return;
            }

            _saveData = root.Economy;

            if (_saveData.PlayerInventory == null)
                _saveData.PlayerInventory = new InventoryState();

            if (_saveData.HiddenCompartment == null)
                _saveData.HiddenCompartment = new InventoryState();

            if (_saveData.CityInventories == null)
                _saveData.CityInventories = new List<CityInventoryState>();

            foreach (CityInventoryState cityState in _saveData.CityInventories)
            {
                if (cityState == null) continue;
                if (cityState.Inventory == null)
                    cityState.Inventory = new InventoryState();
                if (cityState.GuildInventory == null)
                    cityState.GuildInventory = new InventoryState();
            }

            UpdatePlayerStream();
            _hiddenCompartmentStream.Value = CloneInventory(_saveData.HiddenCompartment);

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
