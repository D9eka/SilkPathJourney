using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events;
using Internal.Scripts.Items;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using Plugins.Zenject.Source.Install.Contexts;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Npc.Editor
{
    public class NpcDebugWindow : EditorWindow
    {
        private NpcLifeSimulator _simulator;
        private DayTracker _dayTracker;
        private ItemCatalog _itemCatalog;
        private ICityNodeResolver _cityNodeResolver;

        private Vector2 _agentListScroll;
        private Vector2 _detailsScroll;
        private Vector2 _tradeLogScroll;
        private Vector2 _selectedLogScroll;
        private int _selectedAgentIndex = -1;

        [MenuItem("SPJ/Debug/NPC Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<NpcDebugWindow>("NPC Debug");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use this window.", MessageType.Info);
                return;
            }

            if (!TryResolve())
            {
                EditorGUILayout.HelpBox("Cannot resolve dependencies from Zenject.", MessageType.Error);
                return;
            }

            IReadOnlyList<NpcCaravanAgent> agents = _simulator.Agents;

            EditorGUILayout.LabelField(
                $"Current Day: {_dayTracker.CurrentDay} | Agents: {agents.Count}",
                EditorStyles.boldLabel);

            EditorGUILayout.Space(4);

            DrawAgentListAndDetails(agents);
            EditorGUILayout.Space(4);
            DrawActions(agents);
            EditorGUILayout.Space(4);
            DrawTradeLogs(agents);
        }

        private void DrawAgentListAndDetails(IReadOnlyList<NpcCaravanAgent> agents)
        {
            float halfWidth = position.width / 2f - 8f;

            EditorGUILayout.BeginHorizontal();

            // Left column: agent list
            EditorGUILayout.BeginVertical(GUILayout.Width(halfWidth));
            EditorGUILayout.LabelField("Agents", EditorStyles.boldLabel);
            _agentListScroll = EditorGUILayout.BeginScrollView(_agentListScroll, GUILayout.Height(250));

            if (agents.Count == 0)
            {
                EditorGUILayout.LabelField("  (no agents)");
            }
            else
            {
                for (int i = 0; i < agents.Count; i++)
                {
                    NpcCaravanAgent agent = agents[i];
                    if (agent == null) continue;

                    float totalWeight = 0f;
                    if (agent.EconomyState.Inventory?.Items != null)
                    {
                        foreach (var stack in agent.EconomyState.Inventory.Items)
                            totalWeight += stack.Count * _itemCatalog.GetItemWeight(stack.ItemId);
                    }

                    string statusLabel;
                    if (_cityNodeResolver.TryGetCityByNodeId(agent.CurrentNodeId, out CityData city))
                        statusLabel = $"at {city.Id}";
                    else
                        statusLabel = $"-> {agent.DestinationNodeId}";

                    int itemCount = agent.EconomyState.Inventory?.Items?.Count ?? 0;
                    string label = $"[{agent.EconomyState.Archetype}] {agent.EconomyState.Name} | ${agent.EconomyState.Money} | " +
                                   $"Items: {itemCount} | {totalWeight:F1}/{agent.EconomyState.CapacityKg:F0} kg | {statusLabel}";

                    bool selected = _selectedAgentIndex == i;
                    bool newSelected = GUILayout.Toggle(selected, label, EditorStyles.radioButton);
                    if (newSelected && !selected)
                        _selectedAgentIndex = i;
                    else if (!newSelected && selected)
                        _selectedAgentIndex = -1;
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Right column: agent details
            EditorGUILayout.BeginVertical(GUILayout.Width(halfWidth));
            EditorGUILayout.LabelField("Agent Details", EditorStyles.boldLabel);
            _detailsScroll = EditorGUILayout.BeginScrollView(_detailsScroll, GUILayout.Height(250));

            if (_selectedAgentIndex < 0 || _selectedAgentIndex >= agents.Count)
            {
                EditorGUILayout.LabelField("  (select an agent)");
            }
            else
            {
                NpcCaravanAgent agent = agents[_selectedAgentIndex];
                if (agent == null)
                {
                    EditorGUILayout.LabelField("  (agent is null)");
                }
                else
                {
                    EditorGUILayout.LabelField($"  Name: {agent.EconomyState.Name}");
                    EditorGUILayout.LabelField($"  Archetype: {agent.EconomyState.Archetype}");
                    EditorGUILayout.LabelField($"  Experience: {agent.EconomyState.Experience}");
                    if (agent.EconomyState.InDebt)
                        EditorGUILayout.LabelField($"  DEBT: {agent.EconomyState.Debt:F0}");
                    int purchaseCount = agent.EconomyState.Purchases?.Count ?? 0;
                    int knowledgeCount = agent.EconomyState.Knowledge?.Entries?.Count ?? 0;
                    EditorGUILayout.LabelField($"  Purchases: {purchaseCount} | Known modifiers: {knowledgeCount}");
                    EditorGUILayout.LabelField($"  CurrentNodeId: {agent.CurrentNodeId}");
                    EditorGUILayout.LabelField($"  DestinationNodeId: {agent.DestinationNodeId}");
                    EditorGUILayout.LabelField($"  SpeedMetersPerDay: {agent.SpeedMetersPerDay:F1}");
                    EditorGUILayout.LabelField($"  ColorIndex: {agent.ColorIndex} | PrefabIndex: {agent.PrefabIndex}");

                    EditorGUILayout.Space(2);
                    EditorGUILayout.LabelField("  Inventory:", EditorStyles.boldLabel);

                    var items = agent.EconomyState.Inventory?.Items;
                    if (items == null || items.Count == 0)
                    {
                        EditorGUILayout.LabelField("    (empty)");
                    }
                    else
                    {
                        foreach (var stack in items)
                        {
                            string itemName = _itemCatalog.ResolveItemName(stack.ItemId);
                            float weight = stack.Count * _itemCatalog.GetItemWeight(stack.ItemId);
                            EditorGUILayout.LabelField($"    {itemName} x{stack.Count} ({weight:F1} kg)");
                        }
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawActions(IReadOnlyList<NpcCaravanAgent> agents)
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            bool hasSelection = _selectedAgentIndex >= 0 && _selectedAgentIndex < agents.Count;

            GUI.enabled = hasSelection;
            if (GUILayout.Button("Kill Selected"))
            {
                NpcCaravanAgent agent = agents[_selectedAgentIndex];
                _simulator.DebugKillAgent(agent);
                _selectedAgentIndex = -1;
            }
            GUI.enabled = true;

            if (GUILayout.Button("Spawn New"))
            {
                _simulator.DebugSpawnAgent();
            }

            GUI.enabled = hasSelection;
            if (GUILayout.Button("Give $500"))
            {
                agents[_selectedAgentIndex].EconomyState.Money += 500;
            }
            GUI.enabled = true;

            if (GUILayout.Button("Advance 1 Day"))
            {
                _dayTracker.AdvanceDaysAnimated(1);
            }

            if (GUILayout.Button("+10 Days"))
            {
                _dayTracker.AdvanceDaysAnimated(10);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTradeLogs(IReadOnlyList<NpcCaravanAgent> agents)
        {
            float halfWidth = position.width / 2f - 8f;
            IReadOnlyList<string> log = _simulator.ActivityLog;

            EditorGUILayout.BeginHorizontal();

            // Left column: all agents log
            EditorGUILayout.BeginVertical(GUILayout.Width(halfWidth));
            EditorGUILayout.LabelField("All Agents Log", EditorStyles.boldLabel);
            _tradeLogScroll = EditorGUILayout.BeginScrollView(_tradeLogScroll, GUILayout.Height(150));
            if (log.Count == 0)
            {
                EditorGUILayout.LabelField("  (no trades yet)");
            }
            else
            {
                for (int i = log.Count - 1; i >= 0; i--)
                    EditorGUILayout.LabelField($"  {log[i]}");
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Right column: selected agent log
            EditorGUILayout.BeginVertical(GUILayout.Width(halfWidth));
            EditorGUILayout.LabelField("Selected Agent Log", EditorStyles.boldLabel);
            _selectedLogScroll = EditorGUILayout.BeginScrollView(_selectedLogScroll, GUILayout.Height(150));
            if (_selectedAgentIndex >= 0 && _selectedAgentIndex < agents.Count)
            {
                string agentName = agents[_selectedAgentIndex].EconomyState.Name;
                for (int i = log.Count - 1; i >= 0; i--)
                {
                    if (log[i].Contains(agentName))
                        EditorGUILayout.LabelField($"  {log[i]}");
                }
            }
            else
            {
                EditorGUILayout.LabelField("  (select an agent)");
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private bool TryResolve()
        {
            if (_simulator != null && _dayTracker != null && _itemCatalog != null &&
                _cityNodeResolver != null)
                return true;

            SceneContext sceneCtx = Object.FindFirstObjectByType<SceneContext>();
            if (sceneCtx == null) return false;

            var container = sceneCtx.Container;
            _simulator = container.TryResolve<NpcLifeSimulator>();
            _dayTracker = container.TryResolve<DayTracker>();
            _itemCatalog = container.TryResolve<ItemCatalog>();
            _cityNodeResolver = container.TryResolve<ICityNodeResolver>();

            return _simulator != null && _dayTracker != null && _itemCatalog != null
                && _cityNodeResolver != null;
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
                _simulator = null;
                _dayTracker = null;
                _itemCatalog = null;
                _cityNodeResolver = null;
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

    }
}
