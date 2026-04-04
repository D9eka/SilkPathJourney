using Internal.Scripts.Npc.Trading;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class BuyGoodsAction : ICityVisitAction
    {
        private readonly NpcTrader _trader;

        public BuyGoodsAction(NpcTrader trader)
        {
            _trader = trader;
        }

        public void Execute(NpcCityVisitContext ctx)
        {
            if (string.IsNullOrEmpty(ctx.NextTargetNodeId)) return;

            ctx.BuyStats = _trader.HandleDepartureBuyPhase(
                ctx.Economy, ctx.City.Id, ctx.CurrentNodeId, ctx.NextTargetNodeId, ctx.SpeedMetersPerDay);
        }
    }
}
