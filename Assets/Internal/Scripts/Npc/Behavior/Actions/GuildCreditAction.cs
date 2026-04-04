using Internal.Scripts.Npc.Trading;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class GuildCreditAction : ICityVisitAction
    {
        private readonly NpcTrader _trader;

        public GuildCreditAction(NpcTrader trader)
        {
            _trader = trader;
        }

        public void Execute(NpcCityVisitContext ctx) => _trader.EnsureGuildCreditIfNeeded(ctx.Economy, ctx.City.Id);
    }
}
