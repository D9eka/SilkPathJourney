using Internal.Scripts.Npc.Trading;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class SellGoodsAction : ICityVisitAction
    {
        private readonly NpcSellService _sellService;

        public SellGoodsAction(NpcSellService sellService)
        {
            _sellService = sellService;
        }

        public void Execute(NpcCityVisitContext ctx)
        {
            ctx.SellStats = _sellService.HandleArrivalSellPhase(ctx.Economy, ctx.City.Id);
        }
    }
}
