using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.Tariff;

namespace Internal.Scripts.Npc.Behavior.Actions
{
    public sealed class ChargeTariffAction : ICityVisitAction
    {
        private readonly TariffService _tariff;

        public ChargeTariffAction(TariffService tariff)
        {
            _tariff = tariff;
        }

        public void Execute(NpcCityVisitContext ctx) => _tariff.ChargeNpcTariff(ctx.Economy, ctx.City);
    }
}
