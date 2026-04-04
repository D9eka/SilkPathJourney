using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Items;

namespace Internal.Scripts.Npc.Editor
{
    public static class SimulationRouteAnalyzer
    {
        public static readonly CityType[] MajorCityTypes = { CityType.Capital, CityType.TradeHub, CityType.PortCity };

        public static (string from, string to)[] BuildRoutes(EconomyDatabase economyDb)
        {
            var majorCities = economyDb.Cities
                .Where(c => c != null && MajorCityTypes.Contains(c.Type))
                .Select(c => c.Id)
                .ToList();

            var pairs = new List<(string, string)>();
            for (int i = 0; i < majorCities.Count; i++)
            for (int j = i + 1; j < majorCities.Count; j++)
                pairs.Add((majorCities[i], majorCities[j]));

            return pairs.ToArray();
        }

        public static bool IsRouteAnalysisItem(string itemId)
        {
            return !string.Equals(itemId, SuppliesItemId.Value, System.StringComparison.Ordinal);
        }

        public static int EstimateInventoryMarketValue(
            InventoryState inventory,
            string currentNodeId,
            string destinationNodeId,
            EconomyDatabase economyDb,
            CityTradePriceService priceService)
        {
            if (inventory?.Items == null || economyDb == null || priceService == null)
                return 0;

            string cityId = ResolveValuationCityId(currentNodeId, destinationNodeId, economyDb);
            if (string.IsNullOrEmpty(cityId))
                return 0;

            int total = 0;
            foreach (ItemStackState stack in inventory.Items)
            {
                if (stack == null || stack.Count <= 0)
                    continue;

                int sellPrice = priceService.GetPrice(cityId, stack.ItemId, TradePriceKind.SellToCity, false);
                total += sellPrice * stack.Count;
            }

            return total;
        }

        internal static string ResolveValuationCityId(string currentNodeId, string destinationNodeId, EconomyDatabase economyDb)
        {
            if (!string.IsNullOrWhiteSpace(destinationNodeId))
            {
                CityData destinationCity = economyDb.Cities.Find(c =>
                    string.Equals(c.NodeId, destinationNodeId, System.StringComparison.Ordinal));
                if (destinationCity != null)
                    return destinationCity.Id;
            }

            if (!string.IsNullOrWhiteSpace(currentNodeId))
            {
                CityData currentCity = economyDb.Cities.Find(c =>
                    string.Equals(c.NodeId, currentNodeId, System.StringComparison.Ordinal));
                if (currentCity != null)
                    return currentCity.Id;
            }

            return null;
        }

        internal static int CountInventoryUnits(InventoryState inventory)
        {
            if (inventory?.Items == null)
                return 0;

            int total = 0;
            foreach (ItemStackState stack in inventory.Items)
            {
                if (stack != null && stack.Count > 0)
                    total += stack.Count;
            }

            return total;
        }
    }
}
