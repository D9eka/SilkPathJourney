namespace Internal.Scripts.Npc.Behavior
{
    public sealed class NpcCityVisitProcessor
    {
        private readonly NpcBehaviorProfileRegistry _registry;
        private readonly NpcVisitActionFactory _factory;

        public NpcCityVisitProcessor(NpcBehaviorProfileRegistry registry, NpcVisitActionFactory factory)
        {
            _registry = registry;
            _factory = factory;
        }

        public NpcCityVisitContext Process(NpcCityVisitContext context)
        {
            NpcBehaviorProfile profile = _registry.GetProfile(context.Economy.Archetype);
            foreach (NpcVisitActionType actionType in profile.VisitActions)
            {
                ICityVisitAction action = _factory.Create(actionType);
                action.Execute(context);
            }
            return context;
        }
    }
}
