using System;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Languages.Generated;
using Plugins.Zenject.Source.Install.Contexts;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Caravan.Editor
{
    public class CaravanDebugWindow : EditorWindow
    {
        private PlayerResourceRepository _resourceRepo;
        private CaravanDatabase _caravanDb;
        private CompanionService _companionService;

        private CartClass _selectedCartClass = CartClass.Balanced;
        private CartUpgradeLevel _selectedUpgradeLevel;
        private DraftAnimalType _selectedAnimalType;
        private ExtraCartType _selectedExtraCartType;
        private CompanionType _selectedCompanionType;
        private CompanionQuality _selectedCompanionQuality;
        private LanguageType _selectedLanguage;
        private Vector2 _scrollPos;

        [MenuItem("SPJ/Debug/Caravan Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<CaravanDebugWindow>("Caravan Debug");
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

            DrawMainCart();
            EditorGUILayout.Space(8);
            DrawDraftAnimal();
            EditorGUILayout.Space(8);
            DrawExtraCarts();
            EditorGUILayout.Space(8);
            DrawCompanions();

            EditorGUILayout.EndScrollView();
        }

        private bool TryResolve()
        {
            if (_resourceRepo != null && _caravanDb != null) return true;

            var sceneCtx = FindFirstObjectByType<SceneContext>();
            if (sceneCtx == null) return false;

            var container = sceneCtx.Container;
            _resourceRepo = container.TryResolve<PlayerResourceRepository>();
            _caravanDb = container.TryResolve<CaravanDatabase>();
            _companionService = container.TryResolve<CompanionService>();

            if (_resourceRepo != null)
            {
                var state = _resourceRepo.Current;
                if (Enum.TryParse<CartClass>(state.CartClassId, true, out var pc))
                    _selectedCartClass = pc;
                if (Enum.TryParse<CartUpgradeLevel>(state.CartUpgradeLevelId, true, out var pu))
                    _selectedUpgradeLevel = pu;
                if (Enum.TryParse<DraftAnimalType>(state.DraftAnimalId, true, out var pa))
                    _selectedAnimalType = pa;
            }

            return _resourceRepo != null && _caravanDb != null;
        }

        private void DrawMainCart()
        {
            EditorGUILayout.LabelField("Main Cart", EditorStyles.boldLabel);

            _selectedCartClass = (CartClass)EditorGUILayout.EnumPopup("Cart Class", _selectedCartClass);
            _selectedUpgradeLevel = (CartUpgradeLevel)EditorGUILayout.EnumPopup("Upgrade Level", _selectedUpgradeLevel);

            if (GUILayout.Button("Apply"))
            {
                var cartClass = _selectedCartClass;
                var upgradeLevel = _selectedUpgradeLevel;
                _resourceRepo.UpdateResources(s =>
                {
                    s.CartClassId = cartClass.ToString().ToLower();
                    s.CartUpgradeLevelId = upgradeLevel.ToString().ToLower();
                });
            }
        }

        private void DrawDraftAnimal()
        {
            EditorGUILayout.LabelField("Draft Animal", EditorStyles.boldLabel);

            _selectedAnimalType = (DraftAnimalType)EditorGUILayout.EnumPopup("Animal Type", _selectedAnimalType);

            if (GUILayout.Button("Apply"))
            {
                var animalType = _selectedAnimalType;
                _resourceRepo.UpdateResources(s => s.DraftAnimalId = animalType.ToString().ToLower());
            }
        }

        private void DrawExtraCarts()
        {
            EditorGUILayout.LabelField("Extra Carts", EditorStyles.boldLabel);

            var carts = _resourceRepo.Current.Carts;
            if (carts != null && carts.Count > 0)
            {
                for (int i = 0; i < carts.Count; i++)
                {
                    var cart = carts[i];
                    EditorGUILayout.LabelField($"[{i}] {cart.TypeId}  {cart.Durability}/{cart.MaxDurability}");
                }
            }
            else
            {
                EditorGUILayout.LabelField("(none)");
            }

            EditorGUILayout.Space(4);
            _selectedExtraCartType = (ExtraCartType)EditorGUILayout.EnumPopup("Cart Type", _selectedExtraCartType);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                var extraType = _selectedExtraCartType;
                var extraData = _caravanDb.ExtraCarts.Find(e => e.Type == extraType);
                string typeId = extraData != null ? extraData.Id : extraType.ToString().ToLower();
                float dur = extraData != null ? extraData.Durability : 100;
                float supply = extraData != null ? extraData.SuppliesPerDay : 1;
                _resourceRepo.UpdateResources(s => s.Carts.Add(new CartState
                {
                    TypeId = typeId,
                    Durability = dur,
                    MaxDurability = dur,
                    FoodConsumptionPerDay = supply
                }));
            }

            GUI.enabled = carts != null && carts.Count > 0;
            if (GUILayout.Button("Remove Last"))
            {
                _resourceRepo.UpdateResources(s =>
                {
                    if (s.Carts.Count > 0)
                        s.Carts.RemoveAt(s.Carts.Count - 1);
                });
            }

            if (GUILayout.Button("Clear All"))
            {
                _resourceRepo.UpdateResources(s => s.Carts.Clear());
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCompanions()
        {
            EditorGUILayout.LabelField("Companions", EditorStyles.boldLabel);

            var companions = _resourceRepo.Current.Companions;
            if (companions != null && companions.Count > 0)
            {
                for (int i = 0; i < companions.Count; i++)
                {
                    var c = companions[i];
                    string langSuffix = !string.IsNullOrEmpty(c.LanguageId) ? $" Lang: {c.LanguageId}" : "";
                    EditorGUILayout.LabelField($"[{i}] {c.TypeId} {c.Name} ({c.QualityId}) Injured: {c.IsInjured}{langSuffix}");
                }
            }
            else
            {
                EditorGUILayout.LabelField("(none)");
            }

            EditorGUILayout.Space(4);
            _selectedCompanionType = (CompanionType)EditorGUILayout.EnumPopup("Companion Type", _selectedCompanionType);
            _selectedCompanionQuality = (CompanionQuality)EditorGUILayout.EnumPopup("Quality", _selectedCompanionQuality);

            if (_selectedCompanionType == CompanionType.Translator)
                _selectedLanguage = (LanguageType)EditorGUILayout.EnumPopup("Language", _selectedLanguage);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Hire"))
            {
                if (_selectedCompanionType == CompanionType.Translator && _selectedLanguage != LanguageType.None)
                    _companionService?.HireCompanionWithLanguage(_selectedCompanionType, _selectedCompanionQuality,
                        _selectedLanguage);
                else
                    _companionService?.HireCompanion(_selectedCompanionType, _selectedCompanionQuality,
                        Economy.Generated.CultureId.None);
            }

            GUI.enabled = companions != null && companions.Count > 0;
            if (GUILayout.Button("Remove Last"))
            {
                _companionService?.RemoveCompanion(companions.Count - 1);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
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
                _caravanDb = null;
                _companionService = null;
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
                Repaint();
        }
    }
}
