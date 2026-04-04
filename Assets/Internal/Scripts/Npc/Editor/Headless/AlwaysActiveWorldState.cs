using Internal.Scripts.World.State;

namespace Internal.Scripts.Npc.Editor.Headless
{
    sealed class AlwaysActiveWorldState : IWorldSimulationState
    {
        public bool IsActive => true;
    }
}
