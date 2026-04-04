using Internal.Scripts.Npc.Routing;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class LearnKnowledgeAction : ICityVisitAction
    {
        private readonly NpcKnowledgeService _knowledge;

        public LearnKnowledgeAction(NpcKnowledgeService knowledge)
        {
            _knowledge = knowledge;
        }

        public void Execute(NpcCityVisitContext ctx)
        {
            _knowledge.LearnFromVisit(ctx.Economy, ctx.CurrentNodeId);
            _knowledge.PruneExpired(ctx.Economy, ctx.CurrentDay);
        }
    }
}
