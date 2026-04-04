using System.Collections.Generic;
using Internal.Scripts.Npc.Behavior.Phases;

namespace Internal.Scripts.Npc.Behavior
{
    public sealed class NpcDayPhaseFactory
    {
        private readonly Dictionary<NpcDayPhaseType, INpcDayPhase> _phases = new();

        public NpcDayPhaseFactory(
            ContractExpirationPhase contractExpiration,
            ForagePhase forage,
            ConsumptionPhase consumption,
            StarvationPhase starvation)
        {
            _phases[NpcDayPhaseType.ContractExpiration] = contractExpiration;
            _phases[NpcDayPhaseType.Forage] = forage;
            _phases[NpcDayPhaseType.Consumption] = consumption;
            _phases[NpcDayPhaseType.Starvation] = starvation;
        }

        public INpcDayPhase Create(NpcDayPhaseType type) => _phases[type];
    }
}
