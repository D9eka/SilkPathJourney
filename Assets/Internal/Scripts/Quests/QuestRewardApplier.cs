using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.Quests.Data;

namespace Internal.Scripts.Quests
{
    public class QuestRewardApplier
    {
        private readonly OutcomeApplier _outcomeApplier;

        public QuestRewardApplier(OutcomeApplier outcomeApplier)
        {
            _outcomeApplier = outcomeApplier;
        }

        public void ApplyFailConsequences(QuestData quest)
        {
            if (quest?.FailConsequences == null) return;

            foreach (var entry in quest.FailConsequences)
                _outcomeApplier.Apply(entry);
        }
    }
}
