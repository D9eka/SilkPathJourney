using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Save;
using Internal.Scripts.Economy.Save.Models;
using Internal.Scripts.Inventory;
using Internal.Scripts.Items;
using Internal.Scripts.Player;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.Events;
using UnityEngine;

namespace Internal.Scripts.Economy.Cities.Smuggling
{
    public sealed class SmugglingCheckService
    {
        private readonly CaravanUpgradeService _upgradeService;
        private readonly InventoryRepository _inventoryRepo;
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly ItemCatalog _itemCatalog;
        private readonly DayTracker _dayTracker;
        private readonly PlayerSkillRepository _skillRepo;
        private readonly SmugglingDetectionSettings _detectionSettings;
        private readonly SmugglingPenaltySettings _penaltySettings;
        private readonly SmugglingModifierCalculator _modifierCalculator;

        public SmugglingCheckService(
            CaravanUpgradeService upgradeService,
            InventoryRepository inventoryRepo,
            PlayerResourceRepository resourceRepo,
            ItemCatalog itemCatalog,
            DayTracker dayTracker,
            PlayerSkillRepository skillRepo,
            SmugglingDetectionSettings detectionSettings,
            SmugglingPenaltySettings penaltySettings,
            SmugglingModifierCalculator modifierCalculator)
        {
            _upgradeService = upgradeService;
            _inventoryRepo = inventoryRepo;
            _resourceRepo = resourceRepo;
            _itemCatalog = itemCatalog;
            _dayTracker = dayTracker;
            _skillRepo = skillRepo;
            _detectionSettings = detectionSettings;
            _penaltySettings = penaltySettings;
            _modifierCalculator = modifierCalculator;
        }

        public SmugglingCheckResult PerformCheck(CityData city)
        {
            if (!_upgradeService.HasUpgrade(CaravanUpgradeType.HiddenCompartment))
                return SmugglingCheckResult.Skipped;

            InventoryState compartment = _inventoryRepo.GetHiddenCompartment();
            if (compartment.Items == null || compartment.Items.Count == 0)
                return SmugglingCheckResult.Skipped;

            float chance = CalculateDetectionChance();

            if (Random.value >= chance)
                return SmugglingCheckResult.Passed;

            return ApplyPenalties(city);
        }

        private float CalculateDetectionChance()
        {
            PlayerResourceState resources = _resourceRepo.Current;
            float reputationMod = _modifierCalculator.GetReputationModifier(resources.Reputation);

            int tradeSkill = _skillRepo.Current.GetSkill(SkillType.Trade);
            float skillMod = _modifierCalculator.GetSkillModifier(tradeSkill);

            float caughtMod = resources.SmugglingCaughtCount > 0 ? _detectionSettings.CaughtMultiplier : 1.0f;

            return _detectionSettings.BaseChance * reputationMod * skillMod * caughtMod;
        }

        private SmugglingCheckResult ApplyPenalties(CityData city)
        {
            InventoryState compartment = _inventoryRepo.GetHiddenCompartment();
            int confiscatedValue = CalculateCompartmentValue(compartment);

            _inventoryRepo.UpdateHiddenCompartment(c => c.Items.Clear());

            int penalty = Mathf.Max(_penaltySettings.MinPenaltyAmount,
                (int)(confiscatedValue * _penaltySettings.ConfiscationFraction));

            _resourceRepo.UpdateResources(s =>
            {
                s.Money = Mathf.Max(0, s.Money - penalty);
                s.Reputation -= _penaltySettings.ReputationPenalty;
                s.SmugglingCaughtCount++;
                s.CityTradeBans.Add(new CityTradeBan
                {
                    CityId = city.Id,
                    UntilDay = _dayTracker.CurrentDay + _penaltySettings.BanDurationDays
                });
            });

            return SmugglingCheckResult.Caught(confiscatedValue, penalty);
        }

        private int CalculateCompartmentValue(InventoryState compartment)
        {
            int total = 0;
            if (compartment.Items == null)
                return total;

            foreach (ItemStackState stack in compartment.Items)
            {
                if (stack == null)
                    continue;

                total += _itemCatalog.GetItemPrice(stack.ItemId) * stack.Count;
            }

            return total;
        }
    }
}
