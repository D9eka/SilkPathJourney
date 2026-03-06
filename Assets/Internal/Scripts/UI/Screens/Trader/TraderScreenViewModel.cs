using System;
using System.Collections.Generic;
using Internal.Scripts.Config;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using R3;

namespace Internal.Scripts.UI.Screens.Trader
{
    public sealed class TraderScreenViewModel : ScreenViewModelBase
    {
        private readonly PlayerSkillRepository _skillRepository;
        private readonly GameBalanceConfig _config;
        private readonly TraderUICatalog _catalog;
        private readonly UiThemeService _themeService;
        private readonly ReactiveProperty<TraderViewState> _state = new();

        public TraderScreenViewModel(
            PlayerSkillRepository skillRepository,
            GameBalanceConfig config,
            TraderUICatalog catalog,
            UiThemeService themeService)
        {
            _skillRepository = skillRepository;
            _config = config;
            _catalog = catalog;
            _themeService = themeService;
        }

        public UiThemeService ThemeService => _themeService;

        public override ScreenId Id => ScreenId.Trader;
        public Observable<TraderViewState> State => _state;

        protected override void OnOpen(object args)
        {
            BuildState();
        }
        
        protected override void OnClose()
        {
        }

        private void BuildState()
        {
            var profiles = BuildProfiles();
            var skills = BuildSkills();
            _state.Value = new TraderViewState(profiles, skills);
        }

        private List<ProfileEntry> BuildProfiles()
        {
            var list = new List<ProfileEntry>();

            foreach (var item in _catalog.ProfileItems)
                list.Add(new ProfileEntry(item.Header, ""));

            return list;
        }

        private List<SkillViewData> BuildSkills()
        {
            var list = new List<SkillViewData>();
            int maxSkill = _config.MaxSkill;
            PlayerSkillState current = _skillRepository.Current;

            foreach (SkillType type in Enum.GetValues(typeof(SkillType)))
            {
                if (type == SkillType.None)
                    continue;

                if (!_catalog.TryGetSkill(type, out var name, out var desc))
                    continue;

                int value = current.GetSkill(type);
                float progress = maxSkill > 0 ? (float)value / maxSkill : 0f;

                list.Add(new SkillViewData(name, desc, progress, value.ToString()));
            }

            return list;
        }
    }
}
