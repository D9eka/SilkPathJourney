namespace Internal.Scripts.Player
{
    public interface IPlayerStateProvider
    {
        PlayerState State { get; }
        string CurrentNodeId { get; }
    }
}
