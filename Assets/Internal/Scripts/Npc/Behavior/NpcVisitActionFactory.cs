using System.Collections.Generic;
using Internal.Scripts.Npc.Behavior.Actions;

namespace Internal.Scripts.Npc.Behavior
{
    public sealed class NpcVisitActionFactory
    {
        private readonly Dictionary<NpcVisitActionType, ICityVisitAction> _actions = new();

        public NpcVisitActionFactory(
            LearnKnowledgeAction learn,
            ChargeTariffAction tariff,
            DebtRepaymentAction debtRepayment,
            SellGoodsAction sell,
            CompleteContractAction completeContract,
            TakeContractAction takeContract,
            ChooseRouteAction route,
            BuyGoodsAction buy,
            GuildCreditAction credit)
        {
            _actions[NpcVisitActionType.LearnKnowledge] = learn;
            _actions[NpcVisitActionType.ChargeTariff] = tariff;
            _actions[NpcVisitActionType.DebtRepayment] = debtRepayment;
            _actions[NpcVisitActionType.SellGoods] = sell;
            _actions[NpcVisitActionType.CompleteContract] = completeContract;
            _actions[NpcVisitActionType.TakeContract] = takeContract;
            _actions[NpcVisitActionType.ChooseRoute] = route;
            _actions[NpcVisitActionType.BuyGoods] = buy;
            _actions[NpcVisitActionType.GuildCredit] = credit;
        }

        public ICityVisitAction Create(NpcVisitActionType type) => _actions[type];
    }
}
