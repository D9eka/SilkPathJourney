using System.Collections.Generic;

namespace Internal.Scripts.Npc.Behavior
{
    public sealed class NpcDayProcessor
    {
        private readonly NpcBehaviorProfileRegistry _registry;
        private readonly NpcDayPhaseFactory _factory;

        public NpcDayProcessor(NpcBehaviorProfileRegistry registry, NpcDayPhaseFactory factory)
        {
            _registry = registry;
            _factory = factory;
        }

        public List<int> ProcessDay(NpcDayContext context)
        {
            if (context.SuppliesSnapshot == null || context.SuppliesSnapshot.Length < context.Economies.Count)
                context.SuppliesSnapshot = new int[context.Economies.Count];

            for (int i = 0; i < context.Economies.Count; i++)
            {
                NpcBehaviorProfile profile = _registry.GetProfile(context.Economies[i].Archetype);
                foreach (NpcDayPhaseType phaseType in profile.DayPhases)
                {
                    INpcDayPhase phase = _factory.Create(phaseType);
                    phase.Execute(context, i);
                }
            }
            return context.DeadIndices;
        }
    }
}
