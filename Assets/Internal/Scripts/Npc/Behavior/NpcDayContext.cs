using System;
using System.Collections.Generic;
using Internal.Scripts.Npc.Data;

namespace Internal.Scripts.Npc.Behavior
{
    public class NpcDayContext
    {
        public IReadOnlyList<NpcEconomyState> Economies;
        public IReadOnlyList<string> DestinationNodeIds;
        public int CurrentDay;
        public List<int> DeadIndices = new();
        public HashSet<int> ForagedIndices = new();
        public int[] SuppliesSnapshot;
        public Func<int, float> EstimateDaysToCity;
        public Func<double> NextRandom;
    }
}
