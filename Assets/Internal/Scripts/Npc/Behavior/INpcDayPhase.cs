namespace Internal.Scripts.Npc.Behavior
{
    public interface INpcDayPhase
    {
        void Execute(NpcDayContext context, int agentIndex);
    }
}
