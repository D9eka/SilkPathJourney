using System;
using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Modifiers;
using Internal.Scripts.Road.Positioning;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.World.Roads;
using Internal.Scripts.World.State;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.UI.WorldLabel
{
    public class RoadEffectIconSpawner : IInitializable, IDisposable
    {
        private const float ICON_HEIGHT = 1.5f;
        private const int MODIFIERS_PER_ROAD = 2;

        private readonly RoadRuntime[] _roads;
        private readonly WorldCanvas _worldCanvas;
        private readonly WorldStateController _worldStateController;
        private readonly IRoadSidePositionCalculator _roadSidePositionCalculator;
        private readonly EconomyDatabase _economyDatabase;
        private readonly StaticColorController _colorController;
        private readonly ICityNodeResolver _cityResolver;
        private readonly List<RoadLabelView> _labels = new();

        public RoadEffectIconSpawner(
            RoadRuntime[] roads,
            WorldStateController worldStateController,
            WorldCanvas worldCanvas,
            IRoadSidePositionCalculator roadSidePositionCalculator,
            EconomyDatabase economyDatabase,
            StaticColorController colorController,
            ICityNodeResolver cityResolver)
        {
            _roads = roads;
            _worldStateController = worldStateController;
            _worldCanvas = worldCanvas;
            _roadSidePositionCalculator = roadSidePositionCalculator;
            _economyDatabase = economyDatabase;
            _colorController = colorController;
            _cityResolver = cityResolver;
        }

        public void Initialize()
        {
            SpawnLabels();
            _worldStateController.OnStateChange += OnStateChange;
            OnStateChange(_worldStateController.CurrentViewMode);
        }

        public void Dispose()
        {
            _worldStateController.OnStateChange -= OnStateChange;
            foreach (RoadLabelView label in _labels)
            {
                if (label != null)
                    UnityEngine.Object.Destroy(label.gameObject);
            }
            _labels.Clear();
        }

        private void SpawnLabels()
        {
            foreach (RoadRuntime road in _roads)
            {
                if (road == null || road.Data == null) continue;

                List<Vector3> points = road.Data.PointsLocal;
                if (points == null || points.Count == 0) continue;

                int midIndex = points.Count / 2;
                Vector3 localMid = points[midIndex];
                Vector3 worldMid = road.WorldRoot != null
                    ? road.WorldRoot.TransformPoint(localMid)
                    : localMid;

                Vector3 groundSnappedPos = _roadSidePositionCalculator.SnapToGround(worldMid, ICON_HEIGHT);

                RoadLabelView label = _worldCanvas.CreateRoadLabel(
                    groundSnappedPos,
                    $"RoadLabel_{road.Data.RoadId}");

                if (label == null) continue;

                label.SetColorController(_colorController, GetRoadBiome(road.Data));
                label.SetRoad(road);

                AddRandomModifiers(label);

                _labels.Add(label);
            }
        }

        private void AddRandomModifiers(RoadLabelView label)
        {
            List<RoadModifierData> all = _economyDatabase.RoadModifiers;
            if (all == null || all.Count == 0) return;

            int count = Mathf.Min(MODIFIERS_PER_ROAD, all.Count);
            List<int> indices = new(all.Count);
            for (int i = 0; i < all.Count; i++)
                indices.Add(i);

            for (int i = 0; i < count; i++)
            {
                int pick = UnityEngine.Random.Range(i, indices.Count);
                (indices[i], indices[pick]) = (indices[pick], indices[i]);

                RoadModifierData mod = all[indices[i]];
                if (mod == null) continue;

                label.AddIcon(
                    mod.Icon,
                    mod.GetTooltipTitle(),
                    mod.GetTooltipDescription());
            }
        }

        private Biome GetRoadBiome(RoadData data)
        {
            if (_cityResolver.TryGetCityByNodeId(data.StartNodeId, out var city) && city.Biome != Biome.Unknown)
                return city.Biome;
            if (_cityResolver.TryGetCityByNodeId(data.EndNodeId, out city) && city.Biome != Biome.Unknown)
                return city.Biome;
            return Biome.Plains;
        }

        private void OnStateChange(WorldViewMode viewMode)
        {
            bool show = viewMode == WorldViewMode.Strategic;
            foreach (RoadLabelView label in _labels)
            {
                if (label == null) continue;
                if (show) label.Show();
                else label.Hide();
            }
        }
    }
}
