using System.Linq;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using Plugins.Zenject.Source.Install.Contexts;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Quests.Editor
{
    public class QuestDebugWindow : EditorWindow
    {
        private QuestRepository _questRepository;
        private QuestDatabase _questDatabase;
        private SaveRepository _saveRepository;
        private EventDatabase _eventDatabase;
        private ScreenStackService _screenStackService;

        private int _selectedQuestIndex;
        private string[] _questIds;
        private string[] _questLabels;
        private string _flagId = "";
        private int _flagValue = 1;
        private Vector2 _scrollPos;
        private string _eventPrefix = "quest_";

        [MenuItem("SPJ/Debug/Quest Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<QuestDebugWindow>("Quest Debug");
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

            DrawEventFilter();
            EditorGUILayout.Space(8);
            DrawQuestSelector();
            EditorGUILayout.Space(4);

            bool hasQuests = _questIds != null && _questIds.Length > 0;
            string selectedId = hasQuests ? _questIds[_selectedQuestIndex] : null;
            bool isActive = hasQuests && _questRepository.IsActive(selectedId);
            bool isCompleted = hasQuests && _questRepository.IsCompleted(selectedId);
            bool isFailed = hasQuests && _questRepository.IsFailed(selectedId);

            DrawEventActions(selectedId, isActive, isCompleted, isFailed);
            EditorGUILayout.Space(4);
            DrawDirectActions(selectedId, isActive, isCompleted, isFailed);
            EditorGUILayout.Space(8);
            DrawStatus(selectedId, isActive, isCompleted, isFailed);
            EditorGUILayout.Space(8);
            DrawFlags(selectedId, isActive);
            EditorGUILayout.Space(8);
            DrawClearSection();

            EditorGUILayout.EndScrollView();
        }

        private bool TryResolve()
        {
            if (_questRepository != null && _questDatabase != null) return true;

            var sceneCtx = FindFirstObjectByType<SceneContext>();
            if (sceneCtx == null) return false;

            var container = sceneCtx.Container;
            _questRepository = container.TryResolve<QuestRepository>();
            _questDatabase = container.TryResolve<QuestDatabase>();
            _saveRepository = container.TryResolve<SaveRepository>();
            _eventDatabase = container.TryResolve<EventDatabase>();
            _screenStackService = container.TryResolve<ScreenStackService>();

            if (_questRepository != null && _questDatabase != null &&
                _questDatabase.Quests != null && _questDatabase.Quests.Count > 0)
                RebuildQuestList();

            return _questRepository != null && _questDatabase != null;
        }

        private void RebuildQuestList()
        {
            var quests = _questDatabase.Quests;
            _questIds = quests.Select(q => q.Id).ToArray();

            if (_selectedQuestIndex >= _questIds.Length)
                _selectedQuestIndex = Mathf.Max(0, _questIds.Length - 1);

            if (_questRepository == null)
            {
                _questLabels = _questIds;
                return;
            }

            _questLabels = quests.Select(q =>
            {
                string status = "";
                if (_questRepository.IsActive(q.Id)) status = " [ACTIVE]";
                else if (_questRepository.IsCompleted(q.Id)) status = " [DONE]";
                else if (_questRepository.IsFailed(q.Id)) status = " [FAIL]";
                return $"{q.Id}{status}";
            }).ToArray();
        }

        private void DrawEventFilter()
        {
            EditorGUILayout.LabelField("Event Filter", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _eventPrefix = EditorGUILayout.TextField("Prefix", _eventPrefix);

            string current = EventSelector.DebugEventPrefix;
            bool isActive = !string.IsNullOrEmpty(current);

            GUI.backgroundColor = isActive ? new Color(1f, 0.6f, 0.6f) : new Color(0.6f, 1f, 0.6f);
            if (GUILayout.Button(isActive ? "Disable Filter" : "Enable Filter", GUILayout.Width(110)))
            {
                EventSelector.DebugEventPrefix = isActive ? null : _eventPrefix;
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (isActive)
                EditorGUILayout.HelpBox($"Only events starting with \"{current}\" will appear.", MessageType.Warning);
        }

        private void DrawQuestSelector()
        {
            EditorGUILayout.LabelField("Quest", EditorStyles.boldLabel);

            if (_questIds == null || _questIds.Length == 0)
            {
                EditorGUILayout.HelpBox("QuestDatabase is empty. Run SPJ/Import/Quests.", MessageType.Warning);
                return;
            }

            _selectedQuestIndex = EditorGUILayout.Popup("Select Quest", _selectedQuestIndex, _questLabels);
        }

        private void DrawEventActions(string questId, bool isActive, bool isCompleted, bool isFailed)
        {
            if (questId == null || _eventDatabase == null || _screenStackService == null) return;

            EditorGUILayout.LabelField("Trigger Events", EditorStyles.boldLabel);

            if (!isActive && !isCompleted && !isFailed)
            {
                var startEvent = FindEventWithOutcome(EventOutcomeType.StartQuest, questId);
                GUI.enabled = startEvent != null;
                if (GUILayout.Button(startEvent != null
                    ? $"Start Event: {startEvent.Id}"
                    : "Start Event: (not found)"))
                {
                    TriggerEvent(startEvent);
                }
                GUI.enabled = true;
            }

            if (isActive)
            {
                var data = _questDatabase.GetById(questId);
                int stageIdx = _questRepository.GetCurrentStageIndex(questId);

                if (data?.Stages != null && stageIdx >= 0 && stageIdx < data.Stages.Count)
                {
                    var stage = data.Stages[stageIdx];
                    if (!string.IsNullOrEmpty(stage.TriggerEventId))
                    {
                        var stageEvent = _eventDatabase.GetById(stage.TriggerEventId);
                        GUI.enabled = stageEvent != null;
                        if (GUILayout.Button(stageEvent != null
                            ? $"Stage Event: {stage.TriggerEventId}"
                            : $"Stage Event: {stage.TriggerEventId} (not found)"))
                        {
                            TriggerEvent(stageEvent);
                        }
                        GUI.enabled = true;
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Current stage has no trigger event (auto-complete).",
                            EditorStyles.miniLabel);
                    }
                }
            }
        }

        private void DrawDirectActions(string questId, bool isActive, bool isCompleted, bool isFailed)
        {
            if (questId == null) return;

            EditorGUILayout.LabelField("Direct (skip events)", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !isActive && !isCompleted && !isFailed;
            if (GUILayout.Button("Start"))
            {
                _questRepository.StartQuest(questId);
                RebuildQuestList();
            }

            GUI.enabled = isActive;
            if (GUILayout.Button("Advance"))
            {
                _questRepository.AdvanceQuest(questId);
                RebuildQuestList();
            }

            if (GUILayout.Button("Complete"))
            {
                _questRepository.CompleteQuest(questId);
                RebuildQuestList();
            }

            if (GUILayout.Button("Fail"))
            {
                _questRepository.FailQuest(questId);
                RebuildQuestList();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = isActive;
            if (GUILayout.Button("Track This Quest"))
            {
                _questRepository.TrackQuest(questId);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatus(string questId, bool isActive, bool isCompleted, bool isFailed)
        {
            if (questId == null) return;

            var data = _questDatabase.GetById(questId);

            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);

            string state = isActive ? "Active" :
                isCompleted ? "Completed" :
                isFailed ? "Failed" : "Not Started";

            EditorGUILayout.LabelField("State", state);
            EditorGUILayout.LabelField("Tracked", _questRepository.GetTrackedQuestId() ?? "(none)");

            if (isActive)
            {
                int stageIdx = _questRepository.GetCurrentStageIndex(questId);
                int totalStages = data?.Stages?.Count ?? 0;
                EditorGUILayout.LabelField("Stage", $"{stageIdx} / {totalStages}");

                if (data?.Stages != null && stageIdx < data.Stages.Count)
                {
                    var stage = data.Stages[stageIdx];
                    EditorGUILayout.LabelField("Stage ID", stage.Id);
                    if (!string.IsNullOrEmpty(stage.TriggerEventId))
                        EditorGUILayout.LabelField("Trigger Event", stage.TriggerEventId);
                }

                var entry = _questRepository.GetActiveEntry(questId);
                if (entry?.Flags != null && entry.Flags.Count > 0)
                {
                    EditorGUILayout.LabelField("Flags:");
                    EditorGUI.indentLevel++;
                    foreach (var flag in entry.Flags)
                        EditorGUILayout.LabelField(flag.FlagId, flag.Value.ToString());
                    EditorGUI.indentLevel--;
                }
            }
        }

        private void DrawFlags(string questId, bool isActive)
        {
            if (questId == null || !isActive) return;

            EditorGUILayout.LabelField("Set Flag", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _flagId = EditorGUILayout.TextField("Flag ID", _flagId);
            _flagValue = EditorGUILayout.IntField("Value", _flagValue);
            if (GUILayout.Button("Set", GUILayout.Width(50)))
            {
                if (!string.IsNullOrWhiteSpace(_flagId))
                    _questRepository.SetFlag(questId, _flagId, _flagValue);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawClearSection()
        {
            EditorGUILayout.LabelField("Danger Zone", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Clear All Quest Data"))
            {
                if (EditorUtility.DisplayDialog("Clear Quest Data",
                    "This will remove all active, completed, and failed quests from save data. Continue?",
                    "Clear", "Cancel"))
                {
                    var questData = _saveRepository?.Data?.Quests;
                    if (questData != null)
                    {
                        questData.ActiveQuests.Clear();
                        questData.CompletedQuestIds.Clear();
                        questData.FailedQuestIds.Clear();
                        questData.TrackedQuestId = null;
                        _saveRepository.Save();
                        RebuildQuestList();
                        Debug.Log("[SPJ] Quest data cleared.");
                    }
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private EventData FindEventWithOutcome(EventOutcomeType type, string param)
        {
            if (_eventDatabase?.Events == null) return null;

            foreach (var evt in _eventDatabase.Events)
            {
                if (evt.Choices == null) continue;
                foreach (var choice in evt.Choices)
                {
                    if (choice.Outcomes == null) continue;
                    foreach (var outcome in choice.Outcomes)
                    {
                        if (outcome.Type == type && outcome.Param == param)
                            return evt;
                    }
                }
            }

            return null;
        }

        private void TriggerEvent(EventData eventData)
        {
            if (eventData == null) return;
            var args = new EventTriggerArgs(eventData, null, false);
            _screenStackService.TryOpen(ScreenId.Event, args, out _);
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
                _questRepository = null;
                _questDatabase = null;
                _saveRepository = null;
                _eventDatabase = null;
                _screenStackService = null;
                _questIds = null;
                _questLabels = null;
                _selectedQuestIndex = 0;
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
                Repaint();
        }
    }
}
