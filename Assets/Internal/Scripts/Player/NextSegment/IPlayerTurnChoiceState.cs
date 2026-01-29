namespace Internal.Scripts.Player.NextSegment
{
    public interface IPlayerTurnChoiceState
    {
        bool IsChoosingTurn { get; }
        string CurrentTurnNodeId { get; }
    }
}
