using Internal.Scripts.Npc.Trading;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class DebtRepaymentAction : ICityVisitAction
    {
        private readonly NpcTrader _trader;

        public DebtRepaymentAction(NpcTrader trader)
        {
            _trader = trader;
        }

        public void Execute(NpcCityVisitContext ctx)
        {
            _trader.HandleDebtRepayment(ctx.Economy, ctx.City.Id);
        }
    }
}
