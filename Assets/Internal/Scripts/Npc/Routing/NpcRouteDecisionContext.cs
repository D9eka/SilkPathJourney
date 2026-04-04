using Internal.Scripts.Npc.Data;

namespace Internal.Scripts.Npc.Routing
{
    public readonly struct NpcRouteDecisionContext
    {
        public NpcRouteDecisionContext(NpcEconomyState economyState, string currentNodeId,
            string currentCityId, float speedMetersPerDay, int currentDay)
        {
            EconomyState = economyState;
            CurrentNodeId = currentNodeId;
            CurrentCityId = currentCityId;
            SpeedMetersPerDay = speedMetersPerDay;
            CurrentDay = currentDay;
        }

        public NpcEconomyState EconomyState { get; }
        public string CurrentNodeId { get; }
        public string CurrentCityId { get; }
        public float SpeedMetersPerDay { get; }
        public int CurrentDay { get; }
    }
}
