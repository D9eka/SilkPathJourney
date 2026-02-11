using System;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Save;
using R3;
using Zenject;

namespace Internal.Scripts.Economy
{
    public sealed class PlayerResourceRepository : IInitializable, IDisposable
    {
        private readonly SaveRepository _saveRepository;
        private readonly ReactiveProperty<PlayerResourceState> _state = new(new PlayerResourceState());
        private bool _isLoaded;

        public PlayerResourceRepository(SaveRepository saveRepository)
        {
            _saveRepository = saveRepository;
        }

        public void Initialize()
        {
            EnsureLoaded();
        }

        public Observable<PlayerResourceState> StateStream => _state;
        public PlayerResourceState Current => _state.Value;

        public void UpdateResources(Action<PlayerResourceState> mutator)
        {
            if (mutator == null)
                return;

            EnsureLoaded();
            mutator(_state.Value);
            _saveRepository.Save();
            NotifyChanged();
        }

        public void AddCart(float capacity, float durability = 100f)
        {
            UpdateResources(s => s.Carts.Add(new CartState
            {
                Capacity = capacity,
                Durability = durability,
                MaxDurability = durability
            }));
        }

        public void RemoveCart(int index)
        {
            UpdateResources(s =>
            {
                if (index >= 0 && index < s.Carts.Count)
                    s.Carts.RemoveAt(index);
            });
        }

        private void EnsureLoaded()
        {
            if (_isLoaded)
                return;

            SaveData root = _saveRepository.Data;
            if (root.Economy == null)
                root.Economy = new EconomySaveData();

            if (root.Economy.PlayerResources == null)
                root.Economy.PlayerResources = new PlayerResourceState();

            _state.Value = root.Economy.PlayerResources;
            _isLoaded = true;
        }

        private void NotifyChanged()
        {
            _state.ForceNotify();
        }

        public void Dispose()
        {
            _state?.Dispose();
        }
    }
}
