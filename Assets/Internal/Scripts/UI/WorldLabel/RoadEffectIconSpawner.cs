using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Modifiers;
using Internal.Scripts.Road.Positioning;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.World.State;
using Internal.Scripts.WorldModifiers;
using R3;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Zenject;

namespace Internal.Scripts.UI.WorldLabel
{
    public class RoadEffectIconSpawner : IInitializable, IDisposable
    {
        private const float ICON_HEIGHT = 1.5f;

        private readonly RoadRuntime[] _roads;
        private readonly WorldCanvas _worldCanvas;
        private readonly WorldStateController _worldStateController;
        private readonly IRoadSidePositionCalculator _roadSidePositionCalculator;
        private readonly EconomyDatabase _economyDatabase;
        private readonly StaticColorController _colorController;
        private readonly ICityNodeResolver _cityResolver;
        private readonly WorldModifierRepository _modifierRepo;
        private readonly Events.DayTracker _dayTracker;
        private readonly GameBalanceConfig _balanceConfig;
        private readonly List<RoadLabelView> _labels = new();
        private readonly Dictionary<string, RoadLabelView> _roadLabels = new();
        private IDisposable _modifierSubscription;

        public RoadEffectIconSpawner(
            RoadRuntime[] roads,
            WorldStateController worldStateController,
            WorldCanvas worldCanvas,
            IRoadSidePositionCalculator roadSidePositionCalculator,
            EconomyDatabase economyDatabase,
            StaticColorController colorController,
            ICityNodeResolver cityResolver,
            WorldModifierRepository modifierRepo,
            Events.DayTracker dayTracker,
            GameBalanceConfig balanceConfig)
        {
            _roads = roads;
            _worldStateController = worldStateController;
            _worldCanvas = worldCanvas;
            _roadSidePositionCalculator = roadSidePositionCalculator;
            _economyDatabase = economyDatabase;
            _colorController = colorController;
            _cityResolver = cityResolver;
            _modifierRepo = modifierRepo;
            _dayTracker = dayTracker;
            _balanceConfig = balanceConfig;
        }

        public void Initialize()
        {
            SpawnLabels();
            _worldStateController.OnStateChange += OnStateChange;
            OnStateChange(_worldStateController.CurrentViewMode);
            _modifierSubscription = _modifierRepo.Changed.Subscribe(_ => RefreshAllModifiers());
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        public void Dispose()
        {
            _modifierSubscription?.Dispose();
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _worldStateController.OnStateChange -= OnStateChange;
            foreach (RoadLabelView label in _labels)
            {
                if (label != null)
                    UnityEngine.Object.Destroy(label.gameObject);
            }
            _labels.Clear();
            _roadLabels.Clear();
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

                label.SetColorController(_colorController, _cityResolver.ResolveRoadBiome(road.Data.StartNodeId, road.Data.EndNodeId));
                label.SetRoad(road);

                string roadId = road.Data.RoadId;
                _roadLabels[roadId] = label;
                bool has = AddActiveModifiers(label, roadId);
                label.SetHasModifiers(has);

                _labels.Add(label);
            }
        }

        private bool AddActiveModifiers(RoadLabelView label, string roadId)
        {
            if (label.Modifiers == null) return false;

            var active = _modifierRepo.GetRoadModifiers(roadId);
            bool added = false;
            foreach (var entry in active)
            {
                RoadModifierData mod = _economyDatabase.GetRoadModifier(entry.ModifierId);
                if (mod == null) continue;
                var staleness = ModifierStalenessHelper.GetStaleness(entry.LastSeenDay, _dayTracker.CurrentDay,
                    _balanceConfig.StalenessActualDays, _balanceConfig.StalenessStaleDays);
                float alpha = ModifierStalenessHelper.GetAlpha(staleness);
                if (staleness == ModifierStaleness.Unknown)
                    label.Modifiers.AddIcon(mod.Icon, "???", ModifierStalenessHelper.GetUnknownDescription());
                else
                    label.Modifiers.AddIcon(mod.Icon, mod.GetTooltipTitle(), mod.GetTooltipDescription(), alpha);
                added = true;
            }
            return added;
        }

        private void OnLocaleChanged(Locale _) => RefreshAllModifiers();

        private void RefreshAllModifiers()
        {
            foreach (var kvp in _roadLabels)
            {
                kvp.Value.Modifiers?.ClearIcons();
                bool has = AddActiveModifiers(kvp.Value, kvp.Key);
                kvp.Value.SetHasModifiers(has);
            }
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
