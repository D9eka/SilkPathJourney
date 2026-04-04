using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Routing;
using Internal.Scripts.Npc.Trading;

namespace Internal.Scripts.Npc.Behavior
{
    public class NpcCityVisitContext
    {
        public NpcEconomyState Economy;
        public CityData City;
        public string CurrentNodeId;
        public float SpeedMetersPerDay;
        public int CurrentDay;
        public string NextTargetNodeId;
        public NpcTrader.TradeExecutionStats SellStats;
        public NpcTrader.TradeExecutionStats BuyStats;
        public Func<float> NextRandom;
        public INpcRouteDecisionEnvironment RouteEnvironment;
        public bool Traded;
        public int ContractReward;
    }
}
