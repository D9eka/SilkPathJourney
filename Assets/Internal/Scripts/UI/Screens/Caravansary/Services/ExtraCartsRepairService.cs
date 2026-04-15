using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Shared;

namespace Internal.Scripts.UI.Screens.Caravansary.Services
{
    public sealed class ExtraCartsRepairService : ICityOffering
    {
        private readonly GameBalanceConfig _config;

        public ExtraCartsRepairService(GameBalanceConfig config)
        {
            _config = config;
        }

        public OfferingItem Build(PlayerResourceState state, int money)
        {
            if (state.Carts == null || state.Carts.Count == 0)
                return OfferingItem.Hidden(0);

            float avgDurability = state.Carts.Average(c => c.Durability);
            float avgMax = state.Carts.Average(c => c.MaxDurability);
            float totalMissing = state.Carts.Sum(c => c.MaxDurability - c.Durability);

            int defaultPercent = _config.DefaultRepairPercent;
            int defaultTotalCost = 0;
            foreach (var cart in state.Carts)
            {
                float actual = Math.Min(defaultPercent, cart.MaxDurability - cart.Durability);
                defaultTotalCost += (int)Math.Ceiling(actual) * _config.RepairCostPerPercent;
            }

            int fullMaxCost = (int)Math.Ceiling(totalMissing) * _config.RepairCostPerPercent;
            int maxTotalCost = Math.Min(fullMaxCost, money);
            int maxRepairablePercent = _config.RepairCostPerPercent > 0
                ? money / _config.RepairCostPerPercent
                : 0;

            string title = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravansary_ExtraCarts_Title);
            string durabilityText = $"{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Durability_Prefix)} {avgDurability:F0}/{avgMax:F0}";
            string priceText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravansary_RepairPrice, null, _config.RepairCostPerPercent);

            bool canRepair;
            bool canRepairMax;
            string repairBtnText;
            string repairMaxText;

            if (totalMissing <= 0)
            {
                canRepair = false;
                canRepairMax = false;
                repairBtnText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravansary_Repair_Full);
                repairMaxText = "";
            }
            else
            {
                repairBtnText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravansary_RepairButton, null, defaultPercent, defaultTotalCost);
                int maxDisplayPercent = money >= fullMaxCost
                    ? (int)Math.Ceiling(totalMissing)
                    : maxRepairablePercent;
                int maxDisplayCost = money >= fullMaxCost ? fullMaxCost : maxTotalCost;
                repairMaxText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravansary_RepairMaxButton, null, maxDisplayPercent, maxDisplayCost);
                canRepair = defaultTotalCost > 0 && money >= defaultTotalCost;
                canRepairMax = maxRepairablePercent > 0;
            }

            var entry = new CartRepairEntry(title, durabilityText, priceText,
                repairBtnText, repairMaxText, canRepair, canRepairMax, true);

            return new OfferingItem(entry, 0);
        }

        public void Execute(PlayerResourceRepository repo, bool toMax)
        {
            var state = repo.Current;
            if (state.Carts == null || state.Carts.Count == 0) return;

            int affordable = _config.RepairCostPerPercent > 0
                ? state.Money / _config.RepairCostPerPercent
                : 0;
            int percent;
            if (toMax)
            {
                float maxMissing = state.Carts.Max(c => c.MaxDurability - c.Durability);
                percent = Math.Min((int)Math.Ceiling(maxMissing), affordable);
            }
            else
            {
                percent = _config.DefaultRepairPercent;
            }

            if (percent <= 0) return;

            int totalCost = 0;
            var repairs = new List<(int index, float amount)>();

            for (int i = 0; i < state.Carts.Count; i++)
            {
                float missing = state.Carts[i].MaxDurability - state.Carts[i].Durability;
                float actual = Math.Min(percent, missing);
                if (actual <= 0f) continue;

                int cost = (int)Math.Ceiling(actual) * _config.RepairCostPerPercent;
                totalCost += cost;
                repairs.Add((i, actual));
            }

            if (totalCost <= 0 || state.Money < totalCost) return;

            repo.UpdateResources(s =>
            {
                s.Money -= totalCost;
                foreach (var (index, amount) in repairs)
                {
                    s.Carts[index].Durability = Math.Min(
                        s.Carts[index].Durability + amount,
                        s.Carts[index].MaxDurability);
                }
            });
        }
    }
}
