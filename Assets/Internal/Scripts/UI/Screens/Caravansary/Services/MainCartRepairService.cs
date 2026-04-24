using System;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Shared;

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

            string title = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravansary_MainCart_Title);
            string durabilityText = $"{LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Durability_Prefix)} {cart.Durability:F0}/{cart.MaxDurability:F0}";
            string priceText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravansary_RepairPrice, null, _config.RepairCostPerPercent);

            bool canRepair;
            bool canRepairMax;
            string repairBtnText;
            string repairMaxText;

            if (missing <= 0)
            {
                canRepair = false;
                canRepairMax = false;
                repairBtnText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravansary_Repair_Full);
                repairMaxText = "";
            }
            else
            {
                repairBtnText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravansary_RepairButton, null, defaultPercent, defaultCost);
                repairMaxText = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Caravansary_RepairMaxButton, null, maxPercent, maxCost);
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
    }
}
