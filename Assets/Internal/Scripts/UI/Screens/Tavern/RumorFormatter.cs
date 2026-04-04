using System.Collections.Generic;
using System.Text;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.Rumors;
using Internal.Scripts.UI.Localization;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Tavern
{
    public sealed class RumorFormatter
    {
        public string FormatRumorsText(List<RumorData> rumors)
        {
            if (rumors == null || rumors.Count == 0)
                return ResolveLoc("UI.Tavern.Rumors.Empty", "UI.Tavern.Rumors.Empty");

            var sb = new StringBuilder();
            foreach (var rumor in rumors)
            {
                string line = ResolveLoc("UI.Tavern.Rumors.Entry", "UI.Tavern.Rumors.Entry",
                    ResolveCityName(rumor), rumor.ModifierName);
                sb.AppendLine(line);
            }
            return sb.ToString().TrimEnd();
        }

        public List<RoadInfoEntry> BuildRoadInfoEntries(List<RumorData> rumors, int playerMoney, int rumorCost)
        {
            bool canAfford = playerMoney >= rumorCost;
            string buyText = ResolveLoc("UI.Tavern.Rumors.Buy", "UI.Tavern.Rumors.Buy", rumorCost);
            var result = new List<RoadInfoEntry>(rumors.Count);
            for (int i = 0; i < rumors.Count; i++)
            {
                var rumor = rumors[i];
                string description = ResolveLoc(
                    "UI.Tavern.Rumors.Days", "UI.Tavern.Rumors.Days", rumor.ModifierName, rumor.RemainingDays);
                result.Add(new RoadInfoEntry(ResolveCityName(rumor), description, buyText, canAfford, i));
            }
            return result;
        }

        private static string ResolveCityName(RumorData rumor) =>
            LocalizationService.ResolveString(rumor.City.Name, rumor.City.Id, "TavernRumorCity");

        private static string ResolveLoc(string key, string fallback, params object[] args)
        {
            var localized = new LocalizedString("UI", key);
            return LocalizationService.ResolveString(localized, fallback, key, args);
        }
    }
}
