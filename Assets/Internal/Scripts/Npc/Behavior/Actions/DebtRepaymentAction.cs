using Internal.Scripts.Npc.Trading;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class DebtRepaymentAction : ICityVisitAction
    {
        private readonly NpcGuildTradeService _guildTradeService;

        public DebtRepaymentAction(NpcGuildTradeService guildTradeService)
        {
            _guildTradeService = guildTradeService;
        }

        public void Execute(NpcCityVisitContext ctx)
        {
            _guildTradeService.HandleDebtRepayment(ctx.Economy, ctx.City.Id);
        }
    }
}
