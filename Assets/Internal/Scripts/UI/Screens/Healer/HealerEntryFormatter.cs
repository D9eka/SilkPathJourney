using Internal.Scripts.Caravan;
using Internal.Scripts.Economy.Buildings.Healer;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.UI.Localization;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Healer
{
    public sealed class HealerEntryFormatter
    {
        private readonly HealerService _healerService;
        private readonly CaravanDatabase _caravanDatabase;
        private readonly LocalizedString _healButtonLocalizedString = new("UI", "ui.healer.heal_button");

        public HealerEntryFormatter(HealerService healerService, CaravanDatabase caravanDatabase)
        {
            _healerService = healerService;
            _caravanDatabase = caravanDatabase;
        }

        public HealerEntry Format(int companionIndex, CompanionState companion, int money)
        {
            var typeData = _caravanDatabase.GetCompanionTypeById(companion.TypeId);
            string typeName = typeData != null
                ? LocalizationService.ResolveString(typeData.Name, typeData.Id, "Healer.CompanionType")
                : companion.TypeId;
            string displayName = !string.IsNullOrEmpty(companion.Name)
                ? $"{typeName} {companion.Name}"
                : typeName;

            string qualityKey = companion.QualityId ?? string.Empty;
            int healCost = _healerService.GetHealCost(companion);
            bool canAfford = money >= healCost;

            string statusKey = companion.IsInjured ? "ui.healer.status.injured" : "ui.healer.status.healthy";
            string statusText = LocalizationService.ResolveString(
                new LocalizedString("UI", statusKey),
                companion.IsInjured ? "Injured" : "Healthy",
                "Healer.Status");

            string buttonText = companion.IsInjured
                ? LocalizationService.ResolveString(_healButtonLocalizedString, $"Heal · {healCost} ●", "Healer.Button", healCost)
                : string.Empty;

            return new HealerEntry(companionIndex, displayName, qualityKey, statusText, buttonText, companion.IsInjured, healCost, canAfford);
        }
    }
}
