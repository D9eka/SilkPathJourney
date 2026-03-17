using System;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Shared;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Caravansary.Services
{
    public sealed class MainCartRepairService : ICityOffering
    {
        private readonly GameBalanceConfig _config;

        public MainCartRepairService(GameBalanceConfig config)
        {
            _config = config;
        }

        public OfferingItem Build(PlayerResourceState state, int money)
        {
            var cart = state.PlayerCart;
            float missing = cart.MaxDurability - cart.Durability;
            int defaultPercent = Math.Min(_config.DefaultRepairPercent, (int)Math.Ceiling(missing));
            int defaultCost = defaultPercent * _config.RepairCostPerPercent;
            int affordablePercent = _config.RepairCostPerPercent > 0
                ? money / _config.RepairCostPerPercent
                : 0;
            int maxPercent = Math.Min((int)Math.Ceiling(missing), affordablePercent);
            int maxCost = maxPercent * _config.RepairCostPerPercent;

            string title = ResolveLoc("UI.Caravansary.MainCart.Title", "Main cart");
            string durabilityText = $"{ResolveLoc("UI.Global.Durability.Prefix", "UI.Global.Durability.Prefix")} {cart.Durability:F0}/{cart.MaxDurability:F0}";
            string priceText = ResolveLoc("UI.Caravansary.RepairPrice", "UI.Caravansary.RepairPrice", _config.RepairCostPerPercent);

            bool canRepair;
            bool canRepairMax;
            string repairBtnText;
            string repairMaxText;

            if (missing <= 0)
            {
                canRepair = false;
                canRepairMax = false;
                repairBtnText = ResolveLoc("UI.Caravansary.Repair.Full", "UI.Caravansary.Repair.Full");
                repairMaxText = "";
            }
            else
            {
                repairBtnText = ResolveLoc("UI.Caravansary.RepairButton", "UI.Caravansary.RepairButton", defaultPercent, defaultCost);
                repairMaxText = ResolveLoc("UI.Caravansary.RepairMaxButton", "UI.Caravansary.RepairMaxButton", maxPercent, maxCost);
                canRepair = defaultPercent > 0 && money >= defaultCost;
                canRepairMax = maxPercent > 0;
            }

            var entry = new CartRepairEntry(title, durabilityText, priceText,
                repairBtnText, repairMaxText, canRepair, canRepairMax, true);

            return new OfferingItem(entry, 0);
        }

        public void Execute(PlayerResourceRepository repo, bool toMax)
        {
            var state = repo.Current;
            float missing = state.PlayerCart.MaxDurability - state.PlayerCart.Durability;
            int affordable = _config.RepairCostPerPercent > 0
                ? state.Money / _config.RepairCostPerPercent
                : 0;

            int percent = toMax
                ? Math.Min((int)Math.Ceiling(missing), affordable)
                : Math.Min(_config.DefaultRepairPercent, (int)Math.Ceiling(missing));

            if (percent <= 0) return;

            int cost = percent * _config.RepairCostPerPercent;
            if (state.Money < cost) return;

            repo.UpdateResources(s =>
            {
                s.Money -= cost;
                s.PlayerCart.Durability = Math.Min(
                    s.PlayerCart.Durability + percent,
                    s.PlayerCart.MaxDurability);
            });
        }

        private static string ResolveLoc(string key, string fallback, params object[] args)
        {
            var localized = new LocalizedString("UI", key);
            return LocalizationService.ResolveString(localized, fallback, key, args);
        }
    }
}
