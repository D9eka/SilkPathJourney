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
        private const string MenuPath = "SPJ/Debug/Quest Debug Window";
        private const string WindowTitle = "Quest Debug";

        private const string MsgEnterPlayMode = "Enter Play Mode to use this window.";
        private const string MsgCannotResolve = "Cannot resolve dependencies from Zenject.";
        private const string MsgEmptyDatabase = "QuestDatabase is empty. Run SPJ/Import/Quests.";
        private const string MsgAutoComplete = "Current stage has no trigger event (auto-complete).";

        private const string SectionEventFilter = "Event Filter";
        private const string SectionQuest = "Quest";
        private const string SectionTriggerEvents = "Trigger Events";
        private const string SectionDirectActions = "Direct (skip events)";
        private const string SectionStatus = "Status";
        private const string SectionSetFlag = "Set Flag";
        private const string SectionDangerZone = "Danger Zone";

        private const string LabelPrefix = "Prefix";
        private const string LabelSelectQuest = "Select Quest";
        private const string LabelState = "State";
        private const string LabelTracked = "Tracked";
        private const string LabelStage = "Stage";
        private const string LabelStageId = "Stage ID";
        private const string LabelTriggerEvent = "Trigger Event";
        private const string LabelFlags = "Flags:";
        private const string LabelFlagId = "Flag ID";
        private const string LabelValue = "Value";

        private const string BtnEnableFilter = "Enable Filter";
        private const string BtnDisableFilter = "Disable Filter";
        private const string BtnStart = "Start";
        private const string BtnAdvance = "Advance";
        private const string BtnComplete = "Complete";
        private const string BtnFail = "Fail";
        private const string BtnTrackThis = "Track This Quest";
        private const string BtnSet = "Set";
        private const string BtnClearAll = "Clear All Quest Data";

        private const string StatusActive = " [ACTIVE]";
        private const string StatusDone = " [DONE]";
        private const string StatusFail = " [FAIL]";
        private const string StateActive = "Active";
        private const string StateCompleted = "Completed";
        private const string StateFailed = "Failed";
        private const string StateNotStarted = "Not Started";
        private const string TrackedNone = "(none)";

        private const string PrefixStartEvent = "Start Event: ";
        private const string PrefixStageEvent = "Stage Event: ";
        private const string SuffixNotFound = "(not found)";

        private const string DialogClearTitle = "Clear Quest Data";
        private const string DialogClearMsg = "This will remove all active, completed, and failed quests from save data. Continue?";
        private const string DialogClearOk = "Clear";
        private const string DialogClearCancel = "Cancel";

        private const string LogQuestCleared = "[SPJ] Quest data cleared.";

        private const string DefaultEventPrefix = "quest_";
        private const int DefaultFlagValue = 1;

        private const float FilterButtonWidth = 110f;
        private const float SetButtonWidth = 50f;

        private static readonly Color FilterActiveTint = new(1f, 0.6f, 0.6f);
        private static readonly Color FilterInactiveTint = new(0.6f, 1f, 0.6f);
        private static readonly Color DangerTint = new(1f, 0.4f, 0.4f);

        private QuestRepository _questRepository;
        private QuestDatabase _questDatabase;
        private SaveRepository _saveRepository;
        private EventDatabase _eventDatabase;
        private ScreenStackService _screenStackService;

        private int _selectedQuestIndex;
        private string[] _questIds;
        private string[] _questLabels;
        private string _flagId = "";
        private int _flagValue = DefaultFlagValue;
        private Vector2 _scrollPos;
        private string _eventPrefix = DefaultEventPrefix;

        [MenuItem(MenuPath)]
        public static void ShowWindow()
        {
            GetWindow<QuestDebugWindow>(WindowTitle);
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(MsgEnterPlayMode, MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            if (!TryResolve())
            {
                EditorGUILayout.HelpBox(MsgCannotResolve, MessageType.Error);
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
                if (_questRepository.IsActive(q.Id)) status = StatusActive;
                else if (_questRepository.IsCompleted(q.Id)) status = StatusDone;
                else if (_questRepository.IsFailed(q.Id)) status = StatusFail;
                return $"{q.Id}{status}";
            }).ToArray();
        }

        private void DrawEventFilter()
        {
            EditorGUILayout.LabelField(SectionEventFilter, EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _eventPrefix = EditorGUILayout.TextField(LabelPrefix, _eventPrefix);

            string current = EventSelector.DebugEventPrefix;
            bool isActive = !string.IsNullOrEmpty(current);

            GUI.backgroundColor = isActive ? FilterActiveTint : FilterInactiveTint;
            if (GUILayout.Button(isActive ? BtnDisableFilter : BtnEnableFilter, GUILayout.Width(FilterButtonWidth)))
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
            EditorGUILayout.LabelField(SectionQuest, EditorStyles.boldLabel);

            if (_questIds == null || _questIds.Length == 0)
            {
                EditorGUILayout.HelpBox(MsgEmptyDatabase, MessageType.Warning);
                return;
            }

            _selectedQuestIndex = EditorGUILayout.Popup(LabelSelectQuest, _selectedQuestIndex, _questLabels);
        }

        private void DrawEventActions(string questId, bool isActive, bool isCompleted, bool isFailed)
        {
            if (questId == null || _eventDatabase == null || _screenStackService == null) return;

            EditorGUILayout.LabelField(SectionTriggerEvents, EditorStyles.boldLabel);

            if (!isActive && !isCompleted && !isFailed)
            {
                var startEvent = FindEventWithOutcome(EventOutcomeType.StartQuest, questId);
                GUI.enabled = startEvent != null;
                if (GUILayout.Button(startEvent != null
                    ? $"{PrefixStartEvent}{startEvent.Id}"
                    : $"{PrefixStartEvent}{SuffixNotFound}"))
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
                            ? $"{PrefixStageEvent}{stage.TriggerEventId}"
                            : $"{PrefixStageEvent}{stage.TriggerEventId} {SuffixNotFound}"))
                        {
                            TriggerEvent(stageEvent);
                        }
                        GUI.enabled = true;
                    }
                    else
                    {
                        EditorGUILayout.LabelField(MsgAutoComplete, EditorStyles.miniLabel);
                    }
                }
            }
        }

        private void DrawDirectActions(string questId, bool isActive, bool isCompleted, bool isFailed)
        {
            if (questId == null) return;

            EditorGUILayout.LabelField(SectionDirectActions, EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !isActive && !isCompleted && !isFailed;
            if (GUILayout.Button(BtnStart))
            {
                _questRepository.StartQuest(questId);
                RebuildQuestList();
            }

            GUI.enabled = isActive;
            if (GUILayout.Button(BtnAdvance))
            {
                _questRepository.AdvanceQuest(questId);
                RebuildQuestList();
            }

            if (GUILayout.Button(BtnComplete))
            {
                _questRepository.CompleteQuest(questId);
                RebuildQuestList();
            }

            if (GUILayout.Button(BtnFail))
            {
                _questRepository.FailQuest(questId);
                RebuildQuestList();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = isActive;
            if (GUILayout.Button(BtnTrackThis))
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

            EditorGUILayout.LabelField(SectionStatus, EditorStyles.boldLabel);

            string state = isActive ? StateActive :
                isCompleted ? StateCompleted :
                isFailed ? StateFailed : StateNotStarted;

            EditorGUILayout.LabelField(LabelState, state);
            EditorGUILayout.LabelField(LabelTracked, _questRepository.GetTrackedQuestId() ?? TrackedNone);

            if (isActive)
            {
                int stageIdx = _questRepository.GetCurrentStageIndex(questId);
                int totalStages = data?.Stages?.Count ?? 0;
                EditorGUILayout.LabelField(LabelStage, $"{stageIdx} / {totalStages}");

                if (data?.Stages != null && stageIdx < data.Stages.Count)
                {
                    var stage = data.Stages[stageIdx];
                    EditorGUILayout.LabelField(LabelStageId, stage.Id);
                    if (!string.IsNullOrEmpty(stage.TriggerEventId))
                        EditorGUILayout.LabelField(LabelTriggerEvent, stage.TriggerEventId);
                }

                var entry = _questRepository.GetActiveEntry(questId);
                if (entry?.Flags != null && entry.Flags.Count > 0)
                {
                    EditorGUILayout.LabelField(LabelFlags);
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

            EditorGUILayout.LabelField(SectionSetFlag, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _flagId = EditorGUILayout.TextField(LabelFlagId, _flagId);
            _flagValue = EditorGUILayout.IntField(LabelValue, _flagValue);
            if (GUILayout.Button(BtnSet, GUILayout.Width(SetButtonWidth)))
            {
                if (!string.IsNullOrWhiteSpace(_flagId))
                    _questRepository.SetFlag(questId, _flagId, _flagValue);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawClearSection()
        {
            EditorGUILayout.LabelField(SectionDangerZone, EditorStyles.boldLabel);

            GUI.backgroundColor = DangerTint;
            if (GUILayout.Button(BtnClearAll))
            {
                if (EditorUtility.DisplayDialog(DialogClearTitle, DialogClearMsg, DialogClearOk, DialogClearCancel))
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
                        Debug.Log(LogQuestCleared);
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
