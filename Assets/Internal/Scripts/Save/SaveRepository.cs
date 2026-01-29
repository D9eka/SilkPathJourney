using System.Collections.Generic;
using Internal.Scripts.Economy.Save;

namespace Internal.Scripts.Save
{
    public sealed class SaveRepository
    {
        private readonly ISaveService _saveService;
        private SaveData _data;
        private bool _isLoaded;

        public SaveRepository(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public SaveData Data
        {
            get
            {
                EnsureLoaded();
                return _data;
            }
        }

        public void Save()
        {
            EnsureLoaded();
            _saveService.Save(_data);
        }

        private void EnsureLoaded()
        {
            if (_isLoaded)
                return;

            _data = _saveService.Load() ?? new SaveData
            {
                Economy = new EconomySaveData(),
                Player = new PlayerSaveData()
            };

            Normalize(_data);
            _isLoaded = true;
        }

        private static void Normalize(SaveData data)
        {
            data.Economy ??= new EconomySaveData();
            data.Player ??= new PlayerSaveData();
            data.Economy.PlayerInventory ??= new InventoryState();
            data.Economy.CityInventories ??= new List<CityInventoryState>();

            if (!data.Economy.IsInitialized && data.Economy.CityInventories.Count > 0)
                data.Economy.IsInitialized = true;
        }
    }
}
