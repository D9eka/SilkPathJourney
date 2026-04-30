using System;
using System.Collections.Generic;
using Internal.Scripts.Camera;
using Internal.Scripts.Installers;
using Internal.Scripts.Items;
using Internal.Scripts.Meta;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.Utils;
using R3;
using UnityEngine.Localization.Settings;
using Zenject;

namespace Internal.Scripts.UI.Screens.RunEnd
{
    public sealed class RunEndScreenViewModel : ScreenViewModelBase
    {
        private readonly SaveRepository _saveRepository;
        private readonly ItemCatalog _itemCatalog;
        private readonly SceneLoaderService _sceneLoader;
        private readonly ScreenStackService _screenStackService;
        private readonly SceneReference _mainMenuScene;
        private readonly ResourceIconCatalog _resourceIcons;
        private readonly PersistentProgressService _persistent;

        private readonly ReactiveProperty<RunEndViewState> _state = new();

        public Observable<RunEndViewState> State => _state;
        public ResourceIconCatalog ResourceIcons => _resourceIcons;
        public override ScreenId Id => ScreenId.RunEnd;

        public RunEndScreenViewModel(
            SaveRepository saveRepository,
            ItemCatalog itemCatalog,
            SceneLoaderService sceneLoader,
            ScreenStackService screenStackService,
            ResourceIconCatalog resourceIcons,
            PersistentProgressService persistent,
            [Inject(Id = SceneRefId.MainMenu)] SceneReference mainMenuScene)
        {
            _saveRepository = saveRepository;
            _itemCatalog = itemCatalog;
            _sceneLoader = sceneLoader;
            _screenStackService = screenStackService;
            _resourceIcons = resourceIcons;
            _persistent = persistent;
            _mainMenuScene = mainMenuScene;
        }

        protected override void OnOpen(object args)
        {
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            if (args is RunEndArgs endArgs)
                BuildState(endArgs);
        }

        protected override void OnClose()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            _state.Value = null;
        }

        private void OnLocaleChanged(UnityEngine.Localization.Locale _)
        {
            if (_state.Value != null)
                BuildState(new RunEndArgs(_state.Value.EndType, _state.Value.ReasonKey, _state.Value.Stats));
        }

        private void BuildState(RunEndArgs args)
        {
            string bestDealName = args.Stats?.BestDeal != null && !string.IsNullOrEmpty(args.Stats.BestDeal.ItemId)
                ? _itemCatalog.ResolveItemName(args.Stats.BestDeal.ItemId)
                : "—";

            var languagesNames = new List<string>();
            if (args.Stats?.LanguagesLearnedList != null)
            {
                foreach (string id in args.Stats.LanguagesLearnedList)
                {
                    if (Enum.TryParse<LanguageType>(id, out LanguageType type) && type != LanguageType.None)
                    {
                        var localized = SkillView.ResolveLanguageName(type);
                        languagesNames.Add(LocalizationService.ResolveString(localized, id, $"Language.{id}"));
                    }
                    else
                    {
                        languagesNames.Add(id);
                    }
                }
            }

            int earned = args.Stats?.LegacyEarned ?? 0;
            int bonus = args.Stats?.LegacyBonus ?? 0;
            int total = _persistent.LegacyPoints;

            _state.Value = new RunEndViewState(args.EndType, args.ReasonKey, args.Stats,
                earned, bonus, total,
                bestDealName, languagesNames);
        }

        public void OpenMainMenu()
        {
            _saveRepository.DeleteRun();
            _sceneLoader.LoadScene(_mainMenuScene);
        }

        public void OpenNewGame()
        {
            _saveRepository.DeleteRun();
            _screenStackService.TryOpen(ScreenId.NewGame, out _);
        }

        public void OpenLegacyShop()
        {
            _screenStackService.TryOpen(ScreenId.Legacy, out _);
        }
    }
}
