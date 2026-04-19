using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Player;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using Internal.Scripts.Road.Positioning;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Internal.Scripts.Travel.Pickups
{
    public sealed class PickupSpawner : IInitializable, IFixedTickable, IDisposable
    {
        private const float SpawnChance = 0.4f;
        private const float SideOffset = 1.5f;
        private const float HeightAboveGround = 0.5f;

        private readonly IPlayerStateProvider _player;
        private readonly IPlayerStateEvents _playerEvents;
        private readonly PickupDatabase _database;
        private readonly GameBalanceConfig _balance;
        private readonly IRoadSidePositionCalculator _roadSideCalc;
        private readonly IRoadNetwork _roadNetwork;
        private readonly PickupCollector _collector;

        private readonly List<PickupEntry> _pool = new();
        private readonly List<PickupEntry> _pendingEntries = new();

        public PickupSpawner(
            IPlayerStateProvider player,
            IPlayerStateEvents playerEvents,
            PickupDatabase database,
            GameBalanceConfig balance,
            IRoadSidePositionCalculator roadSideCalc,
            IRoadNetwork roadNetwork,
            PickupCollector collector)
        {
            _player = player;
            _playerEvents = playerEvents;
            _database = database;
            _balance = balance;
            _roadSideCalc = roadSideCalc;
            _roadNetwork = roadNetwork;
            _collector = collector;
        }

        public void Initialize()
        {
            _playerEvents.OnCurrentSegmentChanged += OnSegmentChanged;
        }

        public void Dispose()
        {
            _playerEvents.OnCurrentSegmentChanged -= OnSegmentChanged;
            ClearAll();
        }

        public void FixedTick()
        {
            if (_player.State != PlayerState.Moving)
                return;

            Vector3 playerPos = _player.CurrentPosition;
            int activeCount = 0;

            for (int i = _pendingEntries.Count - 1; i >= 0; i--)
            {
                PickupEntry entry = _pendingEntries[i];
                if (entry.View == null)
                {
                    _pendingEntries.RemoveAt(i);
                    continue;
                }

                float dist = Vector3.Distance(playerPos, entry.WorldPosition);

                if (entry.WasActivated && dist > _balance.PickupDespawnRadius)
                {
                    Log($"Despawn '{entry.Data?.Type}' dist={dist:F1}m");
                    ReturnToPool(entry);
                    _pendingEntries.RemoveAt(i);
                    continue;
                }

                if (entry.View.gameObject.activeSelf)
                {
                    activeCount++;
                }
                else if (entry.WasActivated)
                {
                    Log($"Consumed '{entry.Data?.Type}' at dist={dist:F1}m");
                    ReturnToPool(entry);
                    _pendingEntries.RemoveAt(i);
                    continue;
                }
                else if (dist <= _balance.PickupVisibleRadius && activeCount < _balance.MaxActivePickups)
                {
                    Log($"Activate '{entry.Data?.Type}' dist={dist:F1}m pos={entry.WorldPosition}");
                    entry.View.gameObject.SetActive(true);
                    entry.WasActivated = true;
                    activeCount++;
                }
            }
        }

        private void OnSegmentChanged(string fromNode, string toNode)
        {
            ClearPending();

            if (string.IsNullOrEmpty(fromNode) || string.IsNullOrEmpty(toNode))
            {
                Log($"Segment skipped: from='{fromNode}' to='{toNode}' (empty node id)");
                return;
            }

            if (!_roadNetwork.TryGetSegment(fromNode, toNode, out RoadPathSegment segment))
            {
                LogWarning($"Segment not found: {fromNode} -> {toNode}");
                return;
            }

            float length = segment.LengthMeters;
            float interval = _balance.PickupSpawnIntervalMeters;
            int attempts = 0;
            int placed = 0;

            for (float dist = interval * 0.5f; dist < length; dist += interval)
            {
                attempts++;

                if (UnityEngine.Random.value > SpawnChance)
                    continue;

                PickupData data = PickRandom();
                if (data?.Prefab == null)
                {
                    Log($"Skip roll at dist={dist:F1}m: data='{data?.name ?? "null"}' prefab is null");
                    continue;
                }

                float pct = dist / length;
                bool rightSide = UnityEngine.Random.value > 0.5f;
                RoadLane lane = rightSide ? RoadLane.Right : RoadLane.Left;
                float lateralPct = SideOffset;

                Vector3 worldPos = _roadSideCalc.CalculatePosition(
                    segment, lane, pct, lateralPct, HeightAboveGround);

                PickupView view = GetOrCreateView(data);
                view.transform.position = worldPos;
                view.gameObject.SetActive(false);

                _collector.Register(view);
                _pendingEntries.Add(new PickupEntry { View = view, WorldPosition = worldPos, Data = data });
                placed++;

                Log($"Placed '{data.Type}' at dist={dist:F1}m pct={pct:F2} lane={lane} pos={worldPos}");
            }

            Log($"Segment {fromNode}->{toNode} length={length:F1}m interval={interval}m attempts={attempts} placed={placed}");
        }

        private static void Log(string msg)
        {
            PickupDebugLog.Push(msg);
        }

        private static void LogWarning(string msg)
        {
            string prefixed = "[Pickup] " + msg;
            Debug.LogWarning(prefixed);
            PickupDebugLog.Push("WARN: " + msg);
        }

        private PickupData PickRandom()
        {
            if (_database.Pickups == null || _database.Pickups.Count == 0)
                return null;

            float total = 0f;
            foreach (PickupData d in _database.Pickups)
                total += d.Weight;

            float roll = UnityEngine.Random.value * total;
            float acc = 0f;
            foreach (PickupData d in _database.Pickups)
            {
                acc += d.Weight;
                if (roll <= acc)
                    return d;
            }
            return _database.Pickups[_database.Pickups.Count - 1];
        }

        private PickupView GetOrCreateView(PickupData data)
        {
            for (int i = _pool.Count - 1; i >= 0; i--)
            {
                PickupEntry poolEntry = _pool[i];
                if (poolEntry.View == null)
                {
                    _pool.RemoveAt(i);
                    continue;
                }
                if (!poolEntry.View.gameObject.activeSelf && poolEntry.Data == data)
                {
                    _pool.RemoveAt(i);
                    poolEntry.View.Initialize(data);
                    return poolEntry.View;
                }
            }

            GameObject go = Object.Instantiate(data.Prefab);
            PickupView view = go.GetComponent<PickupView>();
            if (view == null)
                view = go.AddComponent<PickupView>();
            view.Initialize(data);
            return view;
        }

        private void ReturnToPool(PickupEntry entry)
        {
            if (entry.View != null)
            {
                _collector.Unregister(entry.View);
                if (entry.View.gameObject.activeSelf)
                    entry.View.Disable();
            }

            entry.WasActivated = false;
            _pool.Add(entry);
        }

        private void ClearPending()
        {
            foreach (PickupEntry entry in _pendingEntries)
                ReturnToPool(entry);
            _pendingEntries.Clear();
        }

        private void ClearAll()
        {
            ClearPending();
            foreach (PickupEntry entry in _pool)
            {
                if (entry.View != null)
                    Object.Destroy(entry.View.gameObject);
            }
            _pool.Clear();
        }

        private sealed class PickupEntry
        {
            public PickupView View;
            public Vector3 WorldPosition;
            public PickupData Data;
            public bool WasActivated;
        }
    }
}
