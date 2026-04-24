using System;
using Internal.Scripts.Config;
using Internal.Scripts.Player;
using Internal.Scripts.Travel.Pickups;
using Zenject;

namespace Internal.Scripts.Travel.Triggers.Actions
{
    public sealed class PickupSegmentAction : ISegmentTriggerAction, IInitializable, IDisposable
    {
        private readonly PickupSpawner _spawner;
        private readonly IPlayerStateEvents _playerEvents;
        private readonly GameBalanceConfig _balance;

        public int Weight => _balance.SegmentWeights.Pickup;

        public PickupSegmentAction(
            PickupSpawner spawner,
            IPlayerStateEvents playerEvents,
            GameBalanceConfig balance)
        {
            _spawner = spawner;
            _playerEvents = playerEvents;
            _balance = balance;
        }

        public void Initialize() => _playerEvents.OnCurrentSegmentChanged += _spawner.OnSegmentChanged;

        public void Dispose() => _playerEvents.OnCurrentSegmentChanged -= _spawner.OnSegmentChanged;

        public bool CanTrigger() => true;

        public void Trigger() => _spawner.SpawnOne();
    }
}
