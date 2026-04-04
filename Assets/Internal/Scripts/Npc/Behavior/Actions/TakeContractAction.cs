using Internal.Scripts.Npc.Trading;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class TakeContractAction : ICityVisitAction
    {
        private readonly NpcGuildTradeService _guildTradeService;

        public TakeContractAction(NpcGuildTradeService guildTradeService)
        {
            _guildTradeService = guildTradeService;
        }

        public void Execute(NpcCityVisitContext ctx)
        {
            _guildTradeService.TryTakeGuildContract(ctx.Economy, ctx.City.Id, ctx.CurrentNodeId,
                ctx.SpeedMetersPerDay, ctx.NextRandom);
        }
    }
}
