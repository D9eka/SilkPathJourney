using Internal.Scripts.Npc.Trading;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class GuildCreditAction : ICityVisitAction
    {
        private readonly NpcGuildTradeService _guildTradeService;

        public GuildCreditAction(NpcGuildTradeService guildTradeService)
        {
            _guildTradeService = guildTradeService;
        }

        public void Execute(NpcCityVisitContext ctx) => _guildTradeService.HandleGuildCredit(ctx.Economy, ctx.City.Id);
    }
}
