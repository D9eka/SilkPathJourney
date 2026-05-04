using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Meta;
using Internal.Scripts.Meta.Unlocks;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Theme;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.Shop
{
    public sealed class LegacyShopTabViewModel
    {
        private readonly PersistentProgressService _persistent;
        private readonly UnlockRepository _unlockRepository;
        private readonly UiThemeService _themeService;

        public LegacyShopTabViewModel(PersistentProgressService persistent, 
            UnlockRepository unlockRepository, UiThemeService themeService)
        {
            _persistent = persistent;
            _unlockRepository = unlockRepository;
            _themeService = themeService;
        }

        public UiThemeService ThemeService => _themeService;
        public int LegacyPointsTotal => _persistent.LegacyPoints;

        public LegacyShopTabState BuildState()
        {
            return new LegacyShopTabState(BuildUnlockEntries(), _persistent.LegacyPoints);
        }

        public bool TryUnlock(string unlockId)
        {
            var unlock = _unlockRepository.GetById(unlockId);
            if (unlock == null) return false;
            if (!_persistent.TrySpendLegacyPoints(unlock.Cost)) return false;

            _persistent.Unlock(unlockId);
            return true;
        }

        private List<UnlockEntryData> BuildUnlockEntries()
        {
            var result = new List<UnlockEntryData>();
            foreach (var unlock in _unlockRepository.GetAll())
            {
                if (unlock == null) continue;
                string name = unlock.Name != null
                    ? LocalizationService.ResolveString(unlock.Name, unlock.Id, unlock.Id)
                    : unlock.Id;
                string desc = unlock.Description != null
                    ? LocalizationService.ResolveString(unlock.Description, string.Empty, unlock.Id)
                    : string.Empty;
                bool isUnlocked = _persistent.UnlockedIds.Contains(unlock.Id, StringComparer.Ordinal);
                result.Add(new UnlockEntryData(unlock.Id, name, desc, unlock.Cost, isUnlocked, unlock.Category));
            }
            return result;
        }
    }
}
