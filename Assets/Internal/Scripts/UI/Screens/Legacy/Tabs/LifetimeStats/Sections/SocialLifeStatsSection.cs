using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats.Sections
{
    public class SocialLifeStatsSection : LifeStatsSection
    {
        [SerializeField] private TextMeshProUGUI _languagesLearnedText;
        [SerializeField] private TextMeshProUGUI _topLanguageText;
        [SerializeField] private TextMeshProUGUI _friendlyCitiesText;
        [SerializeField] private TextMeshProUGUI _hostileCitiesText;
        [SerializeField] private TextMeshProUGUI _caravansHelpedText;
        [SerializeField] private TextMeshProUGUI _conflictsText;
        [SerializeField] private TextMeshProUGUI _npcMetText;
        
        private LocalizationService.LocalizedTextHandle _languagesLearnedHandle;
        private LocalizationService.LocalizedTextHandle _topLanguageHandle;
        private LocalizationService.LocalizedTextHandle _friendlyCitiesHandle;
        private LocalizationService.LocalizedTextHandle _hostileCitiesHandle;
        private LocalizationService.LocalizedTextHandle _caravansHelpedHandle;
        private LocalizationService.LocalizedTextHandle _conflictsHandle;
        private LocalizationService.LocalizedTextHandle _npcMetHandle;
        
        public override void BindLocalization(LocalizationService localization)
        {
            DisposeBinding();
            HeaderHandle = BindValue(_header, LocUI.UI_LegacyShop_Lifetime_Section_Social, localization);
            _languagesLearnedHandle = BindValue(_languagesLearnedText, LocUI.UI_LegacyShop_Lifetime_LanguagesLearned, localization);
            _topLanguageHandle = BindValue(_topLanguageText, LocUI.UI_LegacyShop_Lifetime_TopLanguage, localization);
            _friendlyCitiesHandle = BindValue(_friendlyCitiesText, LocUI.UI_LegacyShop_Lifetime_FriendlyCities, localization);
            _hostileCitiesHandle = BindValue(_hostileCitiesText, LocUI.UI_LegacyShop_Lifetime_HostileCities, localization);
            _caravansHelpedHandle = BindValue(_caravansHelpedText, LocUI.UI_LegacyShop_Lifetime_CaravansHelped, localization);
            _conflictsHandle = BindValue(_conflictsText, LocUI.UI_LegacyShop_Lifetime_Conflicts, localization);
            _npcMetHandle = BindValue(_npcMetText, LocUI.UI_LegacyShop_Lifetime_NpcMet, localization);
        }

        public override void DisposeBinding()
        {
            base.DisposeBinding();
            _languagesLearnedHandle?.Dispose(); _languagesLearnedHandle = null;
            _topLanguageHandle?.Dispose(); _topLanguageHandle = null;
            _friendlyCitiesHandle?.Dispose(); _friendlyCitiesHandle = null;
            _hostileCitiesHandle?.Dispose(); _hostileCitiesHandle = null;
            _caravansHelpedHandle?.Dispose(); _caravansHelpedHandle = null;
            _conflictsHandle?.Dispose(); _conflictsHandle = null;
            _npcMetHandle?.Dispose(); _npcMetHandle = null;
        }

        public override void Apply(LifetimeStatsViewState state)
        {
            var data = state.Raw;
            SetArg(_languagesLearnedHandle, data.TotalLanguagesLearned);
            SetArg(_topLanguageHandle, state.TopLanguageText);
            SetArg(_friendlyCitiesHandle, data.FriendlyCitiesCount);
            SetArg(_hostileCitiesHandle, data.HostileCitiesCount);
            SetArg(_caravansHelpedHandle, data.CaravansHelpedCount);
            SetArg(_conflictsHandle, data.ConflictsResolvedCount);
            SetArg(_npcMetHandle, data.UniqueNpcMetCount);
        }
    }
}