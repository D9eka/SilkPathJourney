using Internal.Scripts.Npc.Trading;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class TakeContractAction : ICityVisitAction
    {
        private readonly NpcTrader _trader;

        public TakeContractAction(NpcTrader trader)
        {
            _trader = trader;
        }

        public void Execute(NpcCityVisitContext ctx)
        {
            _trader.TryTakeGuildContract(ctx.Economy, ctx.City.Id, ctx.CurrentNodeId,
                ctx.SpeedMetersPerDay, ctx.NextRandom);
        }
    }
}
