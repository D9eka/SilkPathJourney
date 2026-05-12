using System.Linq;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Meta;
using Internal.Scripts.Save;
using Internal.Scripts.Trading;
using Internal.Scripts.Travel.Hazards;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using Plugins.Zenject.Source.Install.Contexts;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.UI.Debug.Editor
{
    public class ScreenBrowserDebugWindow : EditorWindow
    {
        private ScreenStackService _screenStack;
        private SaveRepository _saveRepo;
        private EconomyDatabase _economyDb;
        private EventDatabase _eventDb;
        private HazardDatabase _hazardDb;

        private string[] _eventLabels = System.Array.Empty<string>();
        private string[] _cityLabels = System.Array.Empty<string>();
        private string[] _hazardLabels = System.Array.Empty<string>();

        private int _eventIndex;
        private int _cityIndex;
        private int _hazardIndex;

        [MenuItem("SPJ/Debug/Screen Browser")]
        public static void ShowWindow() => GetWindow<ScreenBrowserDebugWindow>("Screen Browser");

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use", MessageType.Info);
                return;
            }

            if (!TryResolve())
            {
                EditorGUILayout.HelpBox("Scene services unavailable", MessageType.Warning);
                return;
            }

            DrawStatus();
            EditorGUILayout.Space(4);
            DrawArgs();
            EditorGUILayout.Space(4);
            DrawScreenButtons();
            EditorGUILayout.Space(4);
            DrawCloseButtons();
        }

        private bool TryResolve()
        {
            if (_screenStack != null)
                return true;

            SceneContext sceneCtx = FindFirstObjectByType<SceneContext>();
            if (sceneCtx == null) return false;

            var container = sceneCtx.Container;
            _screenStack = container.TryResolve<ScreenStackService>();
            _saveRepo = container.TryResolve<SaveRepository>();
            _economyDb = container.TryResolve<EconomyDatabase>();
            _eventDb = container.TryResolve<EventDatabase>();
            _hazardDb = container.TryResolve<HazardDatabase>();

            if (_screenStack == null) return false;

            RebuildDropdowns();
            return true;
        }

        private void RebuildDropdowns()
        {
            _eventLabels = _eventDb?.Events != null
                ? _eventDb.Events.Select(e => e.Id).ToArray()
                : System.Array.Empty<string>();

            _cityLabels = _economyDb?.Cities != null
                ? _economyDb.Cities.Select(c => c.Id).ToArray()
                : System.Array.Empty<string>();

            _hazardLabels = _hazardDb?.Hazards != null
                ? _hazardDb.Hazards.Select(h => h.name).ToArray()
                : System.Array.Empty<string>();

            if (_cityLabels.Length > 0 && _saveRepo?.Data?.Player?.CurrentNodeId != null)
            {
                string currentNodeId = _saveRepo.Data.Player.CurrentNodeId;
                int idx = _economyDb.Cities.FindIndex(c => c.NodeId == currentNodeId);
                if (idx >= 0) _cityIndex = idx;
            }
        }

        private void DrawStatus()
        {
            string day = _saveRepo?.Data?.Player?.CurrentDay.ToString() ?? "?";
            string nodeId = _saveRepo?.Data?.Player?.CurrentNodeId ?? "?";
            bool hasSave = _saveRepo?.HasActiveRun() ?? false;
            EditorGUILayout.LabelField($"Day: {day} | Node: {nodeId} | Save: {(hasSave ? "yes" : "no")}",
                EditorStyles.miniLabel);
        }

        private void DrawArgs()
        {
            EditorGUILayout.LabelField("Args", EditorStyles.boldLabel);

            if (_eventLabels.Length > 0)
                _eventIndex = EditorGUILayout.Popup("Event", _eventIndex, _eventLabels);
            else
                EditorGUILayout.LabelField("Event", "— no events —");

            if (_cityLabels.Length > 0)
                _cityIndex = EditorGUILayout.Popup("City", _cityIndex, _cityLabels);
            else
                EditorGUILayout.LabelField("City", "— no cities —");

            if (_hazardLabels.Length > 0)
                _hazardIndex = EditorGUILayout.Popup("Hazard", _hazardIndex, _hazardLabels);
            else
                EditorGUILayout.LabelField("Hazard", "— no hazards —");
        }

        private void DrawScreenButtons()
        {
            EditorGUILayout.LabelField("HUD / Meta", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            OpenButton(ScreenId.Hud);
            OpenButton(ScreenId.Pause);
            OpenButton(ScreenId.Settings);
            OpenButton(ScreenId.Journal);
            OpenButton(ScreenId.Archive);
            OpenButton(ScreenId.Legacy);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("City", EditorStyles.boldLabel);
            bool hasCity = _cityLabels.Length > 0;
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(!hasCity))
            {
                OpenButton(ScreenId.EnterCity);
                OpenButton(ScreenId.CityEntryConfirm);
            }
            OpenButton(ScreenId.Tavern);
            OpenButton(ScreenId.Healer);
            OpenButton(ScreenId.Guild);
            OpenButton(ScreenId.Temple);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            OpenButton(ScreenId.Workshop);
            OpenButton(ScreenId.Barracks);
            OpenButton(ScreenId.LanguageSchool);
            OpenButton(ScreenId.Caravansary);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Trade & Caravan", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            OpenButton(ScreenId.Inventory);
            OpenButton(ScreenId.Caravan);
            using (new EditorGUI.DisabledScope(!hasCity))
                OpenButton(ScreenId.Trade);
            OpenButton(ScreenId.Trader);
            OpenButton(ScreenId.TargetSelection);
            OpenButton(ScreenId.Camp);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Events & Quests", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_eventLabels.Length == 0))
                OpenButton(ScreenId.Event);
            using (new EditorGUI.DisabledScope(_hazardLabels.Length == 0))
                OpenButton(ScreenId.HazardQte);
            OpenButton(ScreenId.Quests);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("End", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            OpenButton(ScreenId.RunEnd);
            OpenButton(ScreenId.ConfirmEndRun);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCloseButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Close top"))
                _screenStack.CloseTop();

            if (GUILayout.Button("Close all"))
            {
                while (_screenStack.TopId != ScreenId.None && _screenStack.TopId != ScreenId.Hud)
                    _screenStack.CloseTop();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OpenButton(ScreenId id)
        {
            if (!GUILayout.Button(id.ToString())) return;
            object args = ResolveArgs(id);
            _screenStack.TryOpen(id, args, out ScreenOpenResult result);
            if (result != ScreenOpenResult.Success && result != ScreenOpenResult.AlreadyOpen)
                UnityEngine.Debug.LogWarning($"[ScreenBrowser] {id}: {result}");
        }

        private object ResolveArgs(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Event:
                {
                    EventData evt = GetSelectedEvent();
                    CityData city = GetSelectedCity();
                    return evt != null ? new EventTriggerArgs(evt, city, isAtCity: false) : null;
                }
                case ScreenId.EnterCity:
                {
                    CityData city = GetSelectedCity();
                    return city?.Id;
                }
                case ScreenId.CityEntryConfirm:
                    return GetSelectedCity();
                case ScreenId.Trade:
                {
                    CityData city = GetSelectedCity();
                    return city != null ? (object)new GuildTradeArgs(city.Id) : null;
                }
                case ScreenId.HazardQte:
                    return GetSelectedHazard();
                case ScreenId.RunEnd:
                {
                    RunStatsData stats = _saveRepo?.Data?.RunStats ?? new RunStatsData();
                    return new RunEndArgs(EndType.Victory, "debug", stats);
                }
                case ScreenId.ConfirmEndRun:
                    return (System.Action)(() => { });
                default:
                    return null;
            }
        }

        private EventData GetSelectedEvent()
        {
            if (_eventDb?.Events == null || _eventDb.Events.Count == 0) return null;
            int idx = Mathf.Clamp(_eventIndex, 0, _eventDb.Events.Count - 1);
            return _eventDb.Events[idx];
        }

        private CityData GetSelectedCity()
        {
            if (_economyDb?.Cities == null || _economyDb.Cities.Count == 0) return null;
            int idx = Mathf.Clamp(_cityIndex, 0, _economyDb.Cities.Count - 1);
            return _economyDb.Cities[idx];
        }

        private HazardData GetSelectedHazard()
        {
            if (_hazardDb?.Hazards == null || _hazardDb.Hazards.Count == 0) return null;
            int idx = Mathf.Clamp(_hazardIndex, 0, _hazardDb.Hazards.Count - 1);
            return _hazardDb.Hazards[idx];
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
                _screenStack = null;
                _saveRepo = null;
                _economyDb = null;
                _eventDb = null;
                _hazardDb = null;
                _eventLabels = System.Array.Empty<string>();
                _cityLabels = System.Array.Empty<string>();
                _hazardLabels = System.Array.Empty<string>();
                _eventIndex = 0;
                _cityIndex = 0;
                _hazardIndex = 0;
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
                Repaint();
        }
    }
}
