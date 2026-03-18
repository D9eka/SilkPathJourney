using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events;
using Internal.Scripts.Road.Core;
using Internal.Scripts.Road.Modifiers;
using Plugins.Zenject.Source.Install.Contexts;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.WorldModifiers.Editor
{
    public class ModifierDebugWindow : EditorWindow
    {
        private WorldModifierRepository _repo;
        private EconomyDatabase _economyDb;
        private DayTracker _dayTracker;
        private RoadRuntime[] _roads;

        private Vector2 _scrollPos;
        private Vector2 _cityModScroll;
        private Vector2 _cityLogScroll;
        private Vector2 _roadModScroll;
        private Vector2 _roadLogScroll;
        private int _selectedCityIndex;
        private int _selectedCityModIndex;
        private int _selectedRoadIndex;
        private int _selectedRoadModIndex;
        private string _roadFilter = "";

        private string[] _cityIds;
        private string[] _cityLabels;
        private string[] _cityModLabels;
        private string[] _roadIds;
        private string[] _roadLabels;
        private string[] _roadModLabels;
        private string[] _filteredRoadLabels;
        private int[] _filteredRoadIndices;

        [MenuItem("SPJ/Debug/Modifier Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<ModifierDebugWindow>("Modifier Debug");
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use this window.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            if (!TryResolve())
            {
                EditorGUILayout.HelpBox("Cannot resolve dependencies from Zenject.", MessageType.Error);
                EditorGUILayout.EndScrollView();
                return;
            }

            int currentDay = _dayTracker.CurrentDay;
            EditorGUILayout.LabelField($"Current Day: {currentDay}", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            DrawCityModifiers(currentDay);
            DrawDayLog("City Day Log", _repo.CityDayLog, ref _cityLogScroll);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            DrawRoadModifiers(currentDay);
            DrawDayLog("Road Day Log", _repo.RoadDayLog, ref _roadLogScroll);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            DrawSpawnControls();
            EditorGUILayout.Space(8);
            DrawRevealControls(currentDay);
            EditorGUILayout.Space(8);
            DrawActions();

            EditorGUILayout.EndScrollView();
        }

        private void DrawCityModifiers(int currentDay)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
            EditorGUILayout.LabelField("City Modifiers", EditorStyles.boldLabel);
            _cityModScroll = EditorGUILayout.BeginScrollView(_cityModScroll, GUILayout.Height(150));

            bool any = false;
            foreach (var city in _economyDb.Cities)
            {
                var mods = _repo.GetCityModifiers(city.Id);
                if (mods.Count == 0) continue;
                any = true;

                EditorGUILayout.LabelField($"  {city.Id} ({mods.Count}):");
                foreach (var entry in mods)
                {
                    int daysLeft = entry.StartDay + entry.Duration - currentDay;
                    var staleness = ModifierStalenessHelper.GetStaleness(entry.LastSeenDay, currentDay);
                    EditorGUILayout.LabelField($"    {entry.ModifierId} | day {entry.StartDay} | dur {entry.Duration} | left {daysLeft} | {staleness}");
                }
            }

            if (!any)
                EditorGUILayout.LabelField("  (none)");

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawRoadModifiers(int currentDay)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
            EditorGUILayout.LabelField("Road Modifiers", EditorStyles.boldLabel);
            _roadModScroll = EditorGUILayout.BeginScrollView(_roadModScroll, GUILayout.Height(150));

            bool any = false;
            foreach (var road in _roads)
            {
                if (road?.Data == null) continue;
                string roadId = road.Data.RoadId;
                var mods = _repo.GetRoadModifiers(roadId);
                if (mods.Count == 0) continue;

                any = true;
                EditorGUILayout.LabelField($"  {roadId} ({mods.Count}):");
                foreach (var entry in mods)
                {
                    int daysLeft = entry.StartDay + entry.Duration - currentDay;
                    var staleness = ModifierStalenessHelper.GetStaleness(entry.LastSeenDay, currentDay);
                    EditorGUILayout.LabelField($"    {entry.ModifierId} | day {entry.StartDay} | dur {entry.Duration} | left {daysLeft} | {staleness}");
                }
            }

            if (!any)
                EditorGUILayout.LabelField("  (none)");

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDayLog(string title, List<ModifierChangeEntry> log, ref Vector2 scroll)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.45f));
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(150));

            if (log.Count == 0)
            {
                EditorGUILayout.LabelField("  (no changes)");
            }
            else
            {
                foreach (var entry in log)
                {
                    string prefix = entry.IsAdded ? "+" : "-";
                    EditorGUILayout.LabelField($"  [{prefix}] {entry.ModifierId} @ {entry.LocationId} ({entry.Reason})");
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSpawnControls()
        {
            EditorGUILayout.LabelField("Force Spawn", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _selectedCityIndex = EditorGUILayout.Popup("City", _selectedCityIndex, _cityLabels);
            _selectedCityModIndex = EditorGUILayout.Popup(_selectedCityModIndex, _cityModLabels, GUILayout.Width(120));
            if (GUILayout.Button("Spawn", GUILayout.Width(60)))
            {
                string cityId = _cityIds[_selectedCityIndex];
                var mod = _economyDb.CityModifiers[_selectedCityModIndex];
                _repo.AddCityModifier(cityId, mod.Id, _dayTracker.CurrentDay, mod.RollDuration(), ModifierChangeReason.DebugForceSpawn);
                _repo.MarkCitySeen(cityId, _dayTracker.CurrentDay);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            string newFilter = EditorGUILayout.TextField("Road Filter", _roadFilter);
            if (EditorGUI.EndChangeCheck())
            {
                _roadFilter = newFilter;
                ApplyRoadFilter();
            }

            EditorGUILayout.BeginHorizontal();
            _selectedRoadIndex = EditorGUILayout.Popup("Road", _selectedRoadIndex, _filteredRoadLabels);
            _selectedRoadModIndex = EditorGUILayout.Popup(_selectedRoadModIndex, _roadModLabels, GUILayout.Width(120));
            if (GUILayout.Button("Spawn", GUILayout.Width(60)) && _filteredRoadIndices.Length > 0)
            {
                string roadId = _roadIds[_filteredRoadIndices[_selectedRoadIndex]];
                var mod = _economyDb.RoadModifiers[_selectedRoadModIndex];
                _repo.AddRoadModifier(roadId, mod.Id, _dayTracker.CurrentDay, mod.RollDuration(), ModifierChangeReason.DebugForceSpawn);
                _repo.MarkRoadSeen(roadId, _dayTracker.CurrentDay);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRevealControls(int currentDay)
        {
            EditorGUILayout.LabelField("Reveal (set LastSeenDay = now)", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reveal Selected City"))
            {
                _repo.MarkCitySeen(_cityIds[_selectedCityIndex], currentDay);
            }
            if (GUILayout.Button("Reveal Selected Road") && _filteredRoadIndices.Length > 0)
            {
                _repo.MarkRoadSeen(_roadIds[_filteredRoadIndices[_selectedRoadIndex]], currentDay);
            }
            if (GUILayout.Button("Reveal All"))
            {
                foreach (var city in _economyDb.Cities)
                    _repo.MarkCitySeen(city.Id, currentDay);
                foreach (var road in _roads)
                {
                    if (road?.Data != null)
                        _repo.MarkRoadSeen(road.Data.RoadId, currentDay);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Hide Selected City"))
            {
                _repo.MarkCitySeen(_cityIds[_selectedCityIndex], -1);
            }
            if (GUILayout.Button("Hide Selected Road") && _filteredRoadIndices.Length > 0)
            {
                _repo.MarkRoadSeen(_roadIds[_filteredRoadIndices[_selectedRoadIndex]], -1);
            }
            if (GUILayout.Button("Hide All"))
            {
                foreach (var city in _economyDb.Cities)
                    _repo.MarkCitySeen(city.Id, -1);
                foreach (var road in _roads)
                {
                    if (road?.Data != null)
                        _repo.MarkRoadSeen(road.Data.RoadId, -1);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove All Modifiers"))
            {
                var save = _repo;
                foreach (var city in _economyDb.Cities)
                {
                    var mods = save.GetCityModifiers(city.Id);
                    while (mods.Count > 0)
                    {
                        save.RemoveExpired(int.MaxValue);
                        break;
                    }
                }
                save.RemoveExpired(int.MaxValue);
            }
            if (GUILayout.Button("Advance 1 Day"))
            {
                _dayTracker.AdvanceDays(1);
            }
            EditorGUILayout.EndHorizontal();
        }

        private bool TryResolve()
        {
            if (_repo != null && _economyDb != null) return true;

            var sceneCtx = FindFirstObjectByType<SceneContext>();
            if (sceneCtx == null) return false;

            var container = sceneCtx.Container;
            _repo = container.TryResolve<WorldModifierRepository>();
            _economyDb = container.TryResolve<EconomyDatabase>();
            _dayTracker = container.TryResolve<DayTracker>();
            _roads = container.TryResolve<RoadRuntime[]>();

            if (_repo == null || _economyDb == null || _dayTracker == null)
                return false;

            RebuildLists();
            return true;
        }

        private void RebuildLists()
        {
            _cityIds = _economyDb.Cities.Select(c => c.Id).ToArray();
            _cityLabels = _cityIds;
            _cityModLabels = _economyDb.CityModifiers.Select(m => m.Id).ToArray();

            if (_roads != null)
            {
                var validRoads = _roads.Where(r => r?.Data != null).ToArray();
                _roadIds = validRoads.Select(r => r.Data.RoadId).ToArray();
                _roadLabels = validRoads.Select(r => $"{r.Data.StartNodeId} → {r.Data.EndNodeId}").ToArray();
            }
            else
            {
                _roadIds = new string[0];
                _roadLabels = new string[0];
            }
            ApplyRoadFilter();
            _roadModLabels = _economyDb.RoadModifiers.Select(m => m.Id).ToArray();

            _selectedCityIndex = Mathf.Clamp(_selectedCityIndex, 0, Mathf.Max(0, _cityIds.Length - 1));
            _selectedCityModIndex = Mathf.Clamp(_selectedCityModIndex, 0, Mathf.Max(0, _cityModLabels.Length - 1));
            _selectedRoadModIndex = Mathf.Clamp(_selectedRoadModIndex, 0, Mathf.Max(0, _roadModLabels.Length - 1));
        }

        private void ApplyRoadFilter()
        {
            if (string.IsNullOrEmpty(_roadFilter))
            {
                _filteredRoadLabels = _roadLabels;
                _filteredRoadIndices = Enumerable.Range(0, _roadLabels.Length).ToArray();
            }
            else
            {
                var filter = _roadFilter.ToLowerInvariant();
                var indices = new System.Collections.Generic.List<int>();
                var labels = new System.Collections.Generic.List<string>();
                for (int i = 0; i < _roadLabels.Length; i++)
                {
                    if (_roadLabels[i].ToLowerInvariant().Contains(filter) ||
                        _roadIds[i].ToLowerInvariant().Contains(filter))
                    {
                        indices.Add(i);
                        labels.Add(_roadLabels[i]);
                    }
                }
                _filteredRoadIndices = indices.ToArray();
                _filteredRoadLabels = labels.ToArray();
            }
            _selectedRoadIndex = Mathf.Clamp(_selectedRoadIndex, 0, Mathf.Max(0, _filteredRoadLabels.Length - 1));
        }
    }
}
