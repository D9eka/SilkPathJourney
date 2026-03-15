using System;
using System.Collections.Generic;
using System.Text;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Items;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.Player.Skills;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Args;
using Internal.Scripts.UI.Screens.Quests;
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
        private const string MoneyKey = "UI.Global.Resource.Money";
        private const string FoodKey = "UI.Global.Resource.Food";
        private const string DangerKey = "UI.Global.Resource.Danger";
        private const string DurabilityKey = "UI.Global.Resource.Durability";
        private const string MoraleKey = "UI.Global.Resource.Morale";
        private const string ReputationKey = "UI.Global.Resource.Reputation";

        private readonly ItemCatalog _itemCatalog;
        private readonly TraderUICatalog _catalog;
        private readonly QuestDatabase _questDatabase;

        public EventOutcomeFormatter(ItemCatalog itemCatalog, TraderUICatalog catalog, QuestDatabase questDatabase)
        {
            _itemCatalog = itemCatalog;
            _catalog = catalog;
            _questDatabase = questDatabase;
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
            var questLines = new List<string>();

            foreach (EventOutcomeEntry entry in outcomes)
            {
                string detail = FormatOutcomeDetail(entry);
                if (detail == null) continue;

                if (IsQuestOutcome(entry.Type))
                    questLines.Add(detail);
                else if (entry.Type == EventOutcomeType.RemoveItem || entry.Value < 0)
                    losses.Add(detail);
                else
                    gains.Add(detail);
            }

            foreach (string line in questLines)
                sb.AppendLine(line);

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
                case EventOutcomeType.Morale:
                    return $"{rounded} {ResolveResourceName(MoraleKey, "morale")}";
                case EventOutcomeType.Reputation:
                    return $"{rounded} {ResolveResourceName(ReputationKey, "reputation")}";
                case EventOutcomeType.AddItem:
                    string itemName = _itemCatalog.ResolveItemName(entry.Param);
                    return $"{itemName} ×{rounded}";
                case EventOutcomeType.RemoveItem:
                    string removedName = _itemCatalog.ResolveItemName(entry.Param);
                    return $"{removedName} ×{rounded}";
                case EventOutcomeType.AddSkillXp:
                    if (Enum.TryParse(entry.Param, out SkillType skillType))
                        return $"{_catalog.GetSkillName(skillType)} +{rounded} XP";
                    return $"{entry.Param} +{rounded} XP";
                case EventOutcomeType.ChangeLanguageProficiency:
                    if (Enum.TryParse(entry.Param, true, out LanguageType langType))
                    {
                        string profName = TraderUICatalog.GetProficiencyName((LanguageProficiency)rounded);
                        return $"{_catalog.GetLanguageName(langType)}: {profName}";
                    }
                    return $"{entry.Param}: {TraderUICatalog.GetProficiencyName((LanguageProficiency)rounded)}";
                case EventOutcomeType.StartQuest:
                    return FormatQuestOutcome("UI.Event.Outcome.QuestStarted", "New quest: {quest_name}", entry.Param);
                case EventOutcomeType.AdvanceQuest:
                    return FormatQuestOutcome("UI.Event.Outcome.QuestAdvanced", "Quest updated: {quest_name}", entry.Param);
                case EventOutcomeType.CompleteQuest:
                    return FormatQuestOutcome("UI.Event.Outcome.QuestCompleted", "Quest completed: {quest_name}!", entry.Param);
                case EventOutcomeType.FailQuest:
                    return FormatQuestOutcome("UI.Event.Outcome.QuestFailed", "Quest failed: {quest_name}", entry.Param);
                default:
                    return null;
            }
        }

        private string FormatQuestOutcome(string key, string fallback, string questId)
        {
            var quest = _questDatabase?.GetById(questId);
            string questName = quest != null
                ? LocalizationService.ResolveString(quest.Name, questId, QuestLocContext.QuestName(questId))
                : questId;
            string format = ResolveResourceName(key, fallback);
            return format.Replace("{quest_name}", questName);
        }

        private static bool IsQuestOutcome(EventOutcomeType type)
        {
            return type == EventOutcomeType.StartQuest ||
                   type == EventOutcomeType.AdvanceQuest ||
                   type == EventOutcomeType.CompleteQuest ||
                   type == EventOutcomeType.FailQuest;
        }

        private static string ResolveResourceName(string key, string fallback)
        {
            return LocalizationService.ResolveString(
                new LocalizedString("UI", key), fallback, "ResourceName");
        }
    }
}
