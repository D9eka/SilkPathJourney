using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.Events.Outcomes
{
    public class OutcomeApplier
    {
        private readonly Dictionary<EventOutcomeType, IOutcomeApplier> _appliers = new();

        public OutcomeApplier(
            ResourceApplier resource,
            ItemApplier item,
            CartDurabilityApplier cartDurability,
            CompanionApplier companion,
            SkillXpApplier skill,
            RoadUnlockApplier roadUnlock,
            LanguageProficiencyApplier language,
            SkipDaysApplier skipDays,
            MoraleApplier morale,
            ReputationApplier reputation,
            ExtraCartDurabilityApplier extraCartDurability,
            MainCartDurabilityApplier mainCartDurability,
            RemoveItemApplier removeItem,
            BlockMovementApplier blockMovement,
            CompanionInjuredApplier companionInjured,
            StartQuestApplier startQuest,
            AdvanceQuestApplier advanceQuest,
            CompleteQuestApplier completeQuest,
            FailQuestApplier failQuest,
            SetQuestFlagApplier setQuestFlag,
            RunEndOutcomeApplier runEnd)
        {
            Register(resource);
            Register(item);
            Register(cartDurability);
            Register(companion);
            Register(skill);
            Register(roadUnlock);
            Register(language);
            Register(skipDays);
            Register(morale);
            Register(reputation);
            Register(extraCartDurability);
            Register(mainCartDurability);
            Register(removeItem);
            Register(blockMovement);
            Register(companionInjured);
            Register(startQuest);
            Register(advanceQuest);
            Register(completeQuest);
            Register(failQuest);
            Register(setQuestFlag);
            Register(runEnd);
        }

        public ResourceType? GetAffectedResource(EventOutcomeType type)
        {
            if (_appliers.TryGetValue(type, out var applier) && applier is IResourceOutcomeApplier res)
                return res.GetAffectedResource(type);
            return null;
        }

        public void Apply(EventOutcomeEntry entry)
        {
            if (_appliers.TryGetValue(entry.Type, out var applier))
            {
                applier.Apply(entry);
                return;
            }

            Debug.LogWarning($"[SPJ Events] No outcome applier for {entry.Type}");
        }

        public bool CanAffordAll(List<EventOutcomeEntry> outcomes)
        {
            if (outcomes == null || outcomes.Count == 0) return true;

            Dictionary<EventOutcomeType, float> net = new();
            foreach (var e in outcomes)
            {
                if (e.Type == EventOutcomeType.None) continue;
                net.TryGetValue(e.Type, out float cur);
                net[e.Type] = cur + e.Value;
            }

            foreach (var kvp in net)
            {
                if (kvp.Value >= 0) continue;
                if (_appliers.TryGetValue(kvp.Key, out var applier)
                    && applier is IResourceOutcomeApplier res
                    && !res.CanAfford(kvp.Key, kvp.Value))
                    return false;
            }
            return true;
        }

        private void Register(IOutcomeApplier applier)
        {
            foreach (var type in applier.SupportedTypes)
                _appliers[type] = applier;
        }
    }
}
