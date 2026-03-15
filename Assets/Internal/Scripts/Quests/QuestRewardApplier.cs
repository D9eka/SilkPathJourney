using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Quests.Generated;

namespace Internal.Scripts.Quests
{
    public class QuestRewardApplier
    {
        private readonly OutcomeApplier _outcomeApplier;

        public QuestRewardApplier(OutcomeApplier outcomeApplier)
        {
            _outcomeApplier = outcomeApplier;
        }

        public void ApplyRewards(QuestData quest)
        {
            if (quest?.Rewards == null) return;

            foreach (var reward in quest.Rewards)
            {
                EventOutcomeEntry entry = ToOutcomeEntry(reward);
                if (entry.Type == EventOutcomeType.None) continue;
                _outcomeApplier.Apply(entry);
            }
        }

        public void ApplyFailConsequences(QuestData quest)
        {
            if (quest?.FailConsequences == null) return;

            foreach (var entry in quest.FailConsequences)
                _outcomeApplier.Apply(entry);
        }

        private static EventOutcomeEntry ToOutcomeEntry(QuestRewardData reward)
        {
            return reward.Type switch
            {
                QuestRewardType.Money => new EventOutcomeEntry(EventOutcomeType.Money, "", reward.Value),
                QuestRewardType.Reputation => new EventOutcomeEntry(EventOutcomeType.Reputation, "", reward.Value),
                QuestRewardType.Item => new EventOutcomeEntry(EventOutcomeType.AddItem, reward.Target, reward.Value),
                QuestRewardType.UnlockRoute => new EventOutcomeEntry(EventOutcomeType.UnlockRoad, reward.Target, 0),
                QuestRewardType.Status => new EventOutcomeEntry(EventOutcomeType.SetQuestFlag, reward.Target, 1),
                QuestRewardType.UnlockItem => new EventOutcomeEntry(EventOutcomeType.AddItem, reward.Target, reward.Value > 0 ? reward.Value : 1),
                _ => default
            };
        }
    }
}
