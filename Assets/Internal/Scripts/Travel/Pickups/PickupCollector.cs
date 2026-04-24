using System;
using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.InteractableObjects;
using Internal.Scripts.Player;
using Internal.Scripts.UI.WorldLabel;
using UnityEngine;

namespace Internal.Scripts.Travel.Pickups
{
    public sealed class PickupCollector : IDisposable
    {
        private readonly OutcomeApplier _outcomeApplier;
        private readonly IPlayerStateProvider _player;
        private readonly FloatingRewardSpawner _floatingSpawner;
        private readonly List<PickupView> _registeredViews = new();

        public PickupCollector(
            OutcomeApplier outcomeApplier,
            IPlayerStateProvider player,
            FloatingRewardSpawner floatingSpawner)
        {
            _outcomeApplier = outcomeApplier;
            _player = player;
            _floatingSpawner = floatingSpawner;
        }

        public void Dispose()
        {
            foreach (PickupView view in _registeredViews)
            {
                if (view != null)
                    view.OnClick -= HandleClick;
            }
            _registeredViews.Clear();
        }

        public void Register(PickupView view)
        {
            if (view == null || _registeredViews.Contains(view))
                return;

            view.OnClick += HandleClick;
            _registeredViews.Add(view);
        }

        public void Unregister(PickupView view)
        {
            if (view == null)
                return;

            view.OnClick -= HandleClick;
            _registeredViews.Remove(view);
        }

        private void HandleClick(IInteractableObject interactable)
        {
            if (interactable is not PickupView pickup)
                return;

            PickupData data = pickup.Data;
            if (data == null)
                return;

            Vector3 pickupPos = pickup.transform.position;
            Vector3 targetPos = _player.CurrentPosition;
            pickup.PlayCollectAnimation(targetPos, () => ApplyRewards(data.Rewards, pickupPos));

            Unregister(pickup);
        }

        private void ApplyRewards(List<EventOutcomeEntry> rewards, Vector3 worldPos)
        {
            if (rewards == null || rewards.Count == 0)
                return;

            for (int i = 0; i < rewards.Count; i++)
            {
                EventOutcomeEntry entry = rewards[i];
                _outcomeApplier.Apply(entry);
                _floatingSpawner?.Show(worldPos + Vector3.up * (i * 0.3f), entry);
            }

            LogRewards(rewards);
        }

        private static void LogRewards(List<EventOutcomeEntry> rewards)
        {
            string details = string.Join(", ", rewards.ConvertAll(
                r => $"{r.Type} {(r.Value >= 0 ? "+" : "")}{r.Value}"));
            PickupDebugLog.Push("Collected: " + details);
        }
    }
}
