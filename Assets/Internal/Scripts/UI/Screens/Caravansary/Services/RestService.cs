using System;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Shared;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Caravansary.Services
{
    public sealed class RestService : ICityOffering
    {
        private const int DURABILITY_BONUS = 10;

        private readonly GameBalanceConfig _config;

        public RestService(GameBalanceConfig config)
        {
            _config = config;
        }

        public OfferingItem Build(PlayerResourceState state, int money)
        {
            string title = ResolveLoc("UI.Caravansary.Rest.Title", "Rest");
            string description = ResolveLoc("UI.Caravansary.Rest.Description",
                $"+{DURABILITY_BONUS} main cart durability");
            string buttonText = ResolveLoc("UI.Caravansary.Rest.Button",
                "UI.Caravansary.Rest.Button", _config.RestCost);
            bool canAction = money >= _config.RestCost;

            return new OfferingItem(title, description, buttonText, canAction, 0);
        }

        public void Execute(PlayerResourceRepository repo, bool toMax)
        {
            var state = repo.Current;
            if (state.Money < _config.RestCost) return;

            repo.UpdateResources(s =>
            {
                s.Money -= _config.RestCost;
                s.PlayerCart.Durability = Math.Min(
                    s.PlayerCart.Durability + DURABILITY_BONUS,
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
