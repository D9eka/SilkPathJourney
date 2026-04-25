using System.Collections.Generic;
using System.Text;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.Rumors;
using Internal.Scripts.UI.Localization;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Tavern
{
    public sealed class RumorFormatter
    {
        private readonly EconomyDatabase _economyDb;

        public RumorFormatter(EconomyDatabase economyDb)
        {
            _economyDb = economyDb;
        }

        public string FormatRumorsText(List<RumorData> rumors)
        {
            if (rumors == null || rumors.Count == 0)
                return ResolveLoc("UI.Tavern.Rumors.Empty", "UI.Tavern.Rumors.Empty");

            var sb = new StringBuilder();
            foreach (var rumor in rumors)
            {
                string line = ResolveRumorLine(rumor);
                sb.AppendLine(line);
            }
            return sb.ToString().TrimEnd();
        }

        private string ResolveRumorLine(RumorData rumor)
        {
            var modifier = _economyDb?.GetCityModifier(rumor.ModifierId);
            if (modifier?.RumorLines is { Length: > 0 } lines)
            {
                int seed = rumor.StartDay ^ rumor.ModifierId.GetHashCode();
                int index = ((seed % lines.Length) + lines.Length) % lines.Length;
                string cityName = ResolveCityName(rumor);
                return LocalizationService.ResolveString(lines[index], cityName, "TavernRumorLine", cityName);
            }

            return ResolveLoc("UI.Tavern.Rumors.Entry", "UI.Tavern.Rumors.Entry",
                ResolveCityName(rumor), rumor.ModifierName);
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
                    "UI.Tavern.Rumors.BuyDescription", "UI.Tavern.Rumors.BuyDescription", ResolveCityName(rumor));
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
