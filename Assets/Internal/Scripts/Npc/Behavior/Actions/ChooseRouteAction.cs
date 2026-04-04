using Internal.Scripts.Npc.Routing;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class ChooseRouteAction : ICityVisitAction
    {
        private readonly NpcRouteDecisionService _routeService;

        public ChooseRouteAction(NpcRouteDecisionService routeService)
        {
            _routeService = routeService;
        }

        public void Execute(NpcCityVisitContext ctx)
        {
            var routeCtx = new NpcRouteDecisionContext(
                ctx.Economy, ctx.CurrentNodeId, ctx.City.Id, ctx.SpeedMetersPerDay, ctx.CurrentDay);
            NpcRouteDecisionResult result = _routeService.ChooseNextTarget(routeCtx, ctx.RouteEnvironment, ctx.NextRandom);
            ctx.NextTargetNodeId = result.HasTarget ? result.TargetNodeId : null;
        }
    }
}
