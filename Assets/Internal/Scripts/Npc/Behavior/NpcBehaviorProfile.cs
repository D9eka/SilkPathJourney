using Internal.Scripts.Npc.Data;
using UnityEngine;

namespace Internal.Scripts.Npc.Behavior
{
    [CreateAssetMenu(menuName = "SPJ/NPC/Behavior Profile", fileName = "NpcBehaviorProfile")]
    public class NpcBehaviorProfile : ScriptableObject
    {
        [field: SerializeField] public NpcArchetype Archetype { get; private set; }

        [Header("Day Phases")]
        [field: SerializeField] public NpcDayPhaseType[] DayPhases { get; private set; } =
        {
            NpcDayPhaseType.ContractExpiration,
            NpcDayPhaseType.Forage,
            NpcDayPhaseType.Consumption,
            NpcDayPhaseType.Starvation
        };

        [Header("City Visit Actions")]
        [field: SerializeField] public NpcVisitActionType[] VisitActions { get; private set; } =
        {
            NpcVisitActionType.LearnKnowledge,
            NpcVisitActionType.ChargeTariff,
            NpcVisitActionType.DebtRepayment,
            NpcVisitActionType.SellGoods,
            NpcVisitActionType.CompleteContract,
            NpcVisitActionType.TakeContract,
            NpcVisitActionType.ChooseRoute,
            NpcVisitActionType.BuyGoods,
            NpcVisitActionType.GuildCredit
        };

        public void ApplyImport(NpcArchetype archetype, NpcVisitActionType[] visitActions, NpcDayPhaseType[] dayPhases)
        {
            Archetype = archetype;
            VisitActions = visitActions;
            DayPhases = dayPhases;
        }
    }
}
