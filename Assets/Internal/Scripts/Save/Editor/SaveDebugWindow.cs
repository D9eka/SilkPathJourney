using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities.UI;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Events;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Player;
using Internal.Scripts.Save;
using Plugins.Zenject.Source.Install.Contexts;
using Plugins.Zenject.Source.Main;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Save.Editor
{
    public class SaveDebugWindow : EditorWindow
    {
        private PlayerResourceRepository _resourceRepo;
        private InventoryRepository _inventoryRepo;
        private DayTracker _dayTracker;
        private PlayerController _playerController;
        private SaveRepository _saveRepo;
        private CityViewSpawner _cityViewSpawner;

        private int _selectedTab;
        private readonly string[] _tabNames = { "Economy", "Time", "Inventory", "Position", "Raw JSON" };

        private int _moneyInput;
        private int _skipDaysInput = 1;
        private const int SuppliesQuickAddCount = 50;
        private int _addItemCount = SuppliesQuickAddCount;
        private string _citySearch = string.Empty;
        private Vector2 _scrollPos;
        private Vector2 _cityScrollPos;
        private string _rawJson = string.Empty;

        [MenuItem("SPJ/Debug/Save Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<SaveDebugWindow>("Save Debug");
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                _resourceRepo = null;
                _inventoryRepo = null;
                _dayTracker = null;
                _playerController = null;
                _saveRepo = null;
                _cityViewSpawner = null;
                _rawJson = string.Empty;
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
                Repaint();
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

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            EditorGUILayout.Space(8);

            switch (_selectedTab)
            {
                case 0: DrawEconomy(); break;
                case 1: DrawTime(); break;
                case 2: DrawInventory(); break;
                case 3: DrawPosition(); break;
                case 4: DrawRawJson(); break;
            }

            EditorGUILayout.EndScrollView();
        }

        private bool TryResolve()
        {
            if (_resourceRepo != null) return true;

            var sceneCtx = FindFirstObjectByType<SceneContext>();
            if (sceneCtx == null) return false;

            DiContainer container = sceneCtx.Container;
            _resourceRepo = container.TryResolve<PlayerResourceRepository>();
            _inventoryRepo = container.TryResolve<InventoryRepository>();
            _dayTracker = container.TryResolve<DayTracker>();
            _playerController = container.TryResolve<PlayerController>();
            _saveRepo = container.TryResolve<SaveRepository>();
            _cityViewSpawner = container.TryResolve<CityViewSpawner>();

            return _resourceRepo != null;
        }

        private void DrawEconomy()
        {
            EditorGUILayout.LabelField("Economy", EditorStyles.boldLabel);

            PlayerResourceState state = _resourceRepo.Current;
            EditorGUILayout.LabelField($"Current Money: {state.Money}");
            EditorGUILayout.LabelField($"Morale: {state.Morale:F1}");
            EditorGUILayout.LabelField($"Danger: {state.AccumulatedDanger:F1}");

            EditorGUILayout.Space(4);
            _moneyInput = EditorGUILayout.IntField("Set Money", _moneyInput);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply"))
            {
                int value = _moneyInput;
                _resourceRepo.UpdateResources(s => s.Money = value);
            }
            if (GUILayout.Button("+1000"))
                _resourceRepo.UpdateResources(s => s.Money += 1000);
            if (GUILayout.Button("+5000"))
                _resourceRepo.UpdateResources(s => s.Money += 5000);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTime()
        {
            EditorGUILayout.LabelField("Time", EditorStyles.boldLabel);

            int currentDay = _dayTracker?.CurrentDay ?? 0;
            EditorGUILayout.LabelField($"Current Day: {currentDay}");

            EditorGUILayout.Space(4);
            _skipDaysInput = EditorGUILayout.IntField("Skip Days", _skipDaysInput);

            if (GUILayout.Button("Apply"))
                _dayTracker?.AdvanceDays(_skipDaysInput);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+1"))  _dayTracker?.AdvanceDays(1);
            if (GUILayout.Button("+5"))  _dayTracker?.AdvanceDays(5);
            if (GUILayout.Button("+10")) _dayTracker?.AdvanceDays(10);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawInventory()
        {
            EditorGUILayout.LabelField("Inventory", EditorStyles.boldLabel);

            InventoryState inv = _inventoryRepo?.GetPlayerInventory();
            if (inv != null && inv.Items.Count > 0)
            {
                EditorGUILayout.LabelField("Current stacks:", EditorStyles.miniLabel);
                foreach (ItemStackState stack in inv.Items)
                    EditorGUILayout.LabelField($"  {stack.ItemId}: {stack.Count}");
            }
            else
            {
                EditorGUILayout.LabelField("(empty)");
            }

            EditorGUILayout.Space(4);
            _addItemCount = EditorGUILayout.IntField("Count", _addItemCount);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+Supplies"))
            {
                int count = _addItemCount;
                _inventoryRepo?.UpdatePlayerInventory(s =>
                    InventoryStateMutator.AddItems(s, SuppliesItemId.Value, count));
            }
            if (GUILayout.Button("+50 Supplies"))
            {
                _inventoryRepo?.UpdatePlayerInventory(s =>
                    InventoryStateMutator.AddItems(s, SuppliesItemId.Value, SuppliesQuickAddCount));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPosition()
        {
            EditorGUILayout.LabelField("Position", EditorStyles.boldLabel);

            string currentNode = _playerController?.CurrentNodeId ?? "unknown";
            EditorGUILayout.LabelField($"Current Node: {currentNode}");

            EditorGUILayout.Space(4);
            _citySearch = EditorGUILayout.TextField("Search", _citySearch);

            EditorGUILayout.Space(4);

            IReadOnlyList<CityView> views = _cityViewSpawner?.Views;
            if (views == null || views.Count == 0)
            {
                EditorGUILayout.HelpBox("No cities found (CityViewSpawner not resolved or no cities on scene).", MessageType.Warning);
                return;
            }

            _cityScrollPos = EditorGUILayout.BeginScrollView(_cityScrollPos, GUILayout.Height(300));
            foreach (CityView view in views)
            {
                if (view == null || view.City == null)
                    continue;

                string cityName = view.City.Id;
                string nodeId = view.City.NodeId;

                if (!string.IsNullOrEmpty(_citySearch) &&
                    !cityName.ToLower().Contains(_citySearch.ToLower()) &&
                    !nodeId.ToLower().Contains(_citySearch.ToLower()))
                    continue;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{cityName} ({nodeId})");
                if (GUILayout.Button("Teleport", GUILayout.Width(70)))
                    _playerController?.Teleport(nodeId);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawRawJson()
        {
            EditorGUILayout.LabelField("Raw JSON", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh"))
                _rawJson = _saveRepo != null
                    ? JsonUtility.ToJson(_saveRepo.Data, true)
                    : string.Empty;
            if (GUILayout.Button("Force Save"))
                _saveRepo?.SaveRun();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox("MarketMemory (Dictionary) is skipped by JsonUtility — use SaveRepository.Data.MarketMemory directly for inspection.", MessageType.Warning);
            EditorGUILayout.TextArea(_rawJson, GUILayout.ExpandHeight(true), GUILayout.MinHeight(400));
        }
    }
}
