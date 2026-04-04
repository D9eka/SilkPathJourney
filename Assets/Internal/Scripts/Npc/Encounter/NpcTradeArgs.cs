using Internal.Scripts.Npc.Data;

namespace Internal.Scripts.Npc.Encounter
{
    public sealed class NpcTradeArgs
    {
        public NpcCaravanAgent Agent { get; }
        public string PriceCityId { get; }
        public float Markup { get; }
        public float SuppliesMarkupMultiplier { get; }

        public NpcTradeArgs(NpcCaravanAgent agent, string priceCityId, float markup, float suppliesMarkupMultiplier)
        {
            Agent = agent;
            PriceCityId = priceCityId;
            Markup = markup;
            SuppliesMarkupMultiplier = suppliesMarkupMultiplier;
        }
    }
}
