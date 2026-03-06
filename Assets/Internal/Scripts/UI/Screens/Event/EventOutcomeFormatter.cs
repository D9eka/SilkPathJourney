using System;
using System.Collections.Generic;
using System.Text;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Items;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Args;
using Internal.Scripts.UI.Screens.Trader;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Event
{
    public class EventOutcomeFormatter
    {
        private const string SkillCheckSuccessKey = "UI.Event.Outcome.Text.SkillCheckSuccess";
        private const string SkillCheckFailKey = "UI.Event.Outcome.Text.SkillCheckFail";
        private const string GainKey = "UI.Event.Outcome.Text.Gain";
        private const string LossKey = "UI.Event.Outcome.Text.Loss";
        private const string MoneyKey = "UI.Event.Outcome.Resource.Money";
        private const string FoodKey = "UI.Event.Outcome.Resource.Food";
        private const string DangerKey = "UI.Event.Outcome.Resource.Danger";
        private const string DurabilityKey = "UI.Event.Outcome.Resource.Durability";

        private readonly ItemCatalog _itemCatalog;
        private readonly TraderUICatalog _catalog;

        public EventOutcomeFormatter(ItemCatalog itemCatalog, TraderUICatalog catalog)
        {
            _itemCatalog = itemCatalog;
            _catalog = catalog;
        }

        public string BuildSkillCheckLine(SkillCheckData skillCheck, bool succeeded)
        {
            string skillName = _catalog.GetSkillName(skillCheck.SkillType);
            string key = succeeded ? SkillCheckSuccessKey : SkillCheckFailKey;
            string format = LocalizationService.ResolveString(
                new LocalizedString("UI", key),
                succeeded ? "Skill check {skill_name}: Success!" : "Skill check {skill_name}: Failure!",
                "SkillCheckResult");
            return LocArgRenderer.Format(format, new List<ILocArg>
            {
                new TextLocArg("skill_name", skillName)
            });
        }

        public string BuildOutcomeSummary(List<EventOutcomeEntry> outcomes)
        {
            if (outcomes == null || outcomes.Count == 0)
                return null;

            var sb = new StringBuilder();
            var gains = new List<string>();
            var losses = new List<string>();

            foreach (EventOutcomeEntry entry in outcomes)
            {
                string detail = FormatOutcomeDetail(entry);
                if (detail == null) continue;

                if (entry.Value >= 0)
                    gains.Add(detail);
                else
                    losses.Add(detail);
            }

            if (gains.Count > 0)
            {
                string format = LocalizationService.ResolveString(
                    new LocalizedString("UI", GainKey),
                    "You received: {details}", "OutcomeGain");
                sb.AppendLine(LocArgRenderer.Format(format, new List<ILocArg>
                {
                    new TextLocArg("details", string.Join(", ", gains))
                }));
            }

            if (losses.Count > 0)
            {
                string format = LocalizationService.ResolveString(
                    new LocalizedString("UI", LossKey),
                    "You lost: {details}", "OutcomeLoss");
                sb.AppendLine(LocArgRenderer.Format(format, new List<ILocArg>
                {
                    new TextLocArg("details", string.Join(", ", losses))
                }));
            }

            return sb.Length > 0 ? sb.ToString().TrimEnd() : null;
        }

        private string FormatOutcomeDetail(EventOutcomeEntry entry)
        {
            float abs = Mathf.Abs(entry.Value);
            int rounded = Mathf.RoundToInt(abs);

            switch (entry.Type)
            {
                case EventOutcomeType.Money:
                    return $"{rounded} {ResolveResourceName(MoneyKey, "gold")}";
                case EventOutcomeType.Food:
                    return $"{rounded} {ResolveResourceName(FoodKey, "supplies")}";
                case EventOutcomeType.Danger:
                    return $"{rounded} {ResolveResourceName(DangerKey, "danger")}";
                case EventOutcomeType.CartDurability:
                    return $"{rounded} {ResolveResourceName(DurabilityKey, "durability")}";
                case EventOutcomeType.AddItem:
                    string itemName = _itemCatalog.ResolveItemName(entry.Param);
                    return $"{itemName} ×{rounded}";
                case EventOutcomeType.AddSkillXp:
                    if (Enum.TryParse(entry.Param, out SkillType skillType))
                        return $"{_catalog.GetSkillName(skillType)} +{rounded} XP";
                    return $"{entry.Param} +{rounded} XP";
                default:
                    return null;
            }
        }

        private static string ResolveResourceName(string key, string fallback)
        {
            return LocalizationService.ResolveString(
                new LocalizedString("UI", key), fallback, "ResourceName");
        }
    }
}
