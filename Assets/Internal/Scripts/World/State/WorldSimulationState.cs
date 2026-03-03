using Internal.Scripts.Player;
using Internal.Scripts.Player.NextSegment;

namespace Internal.Scripts.World.State
{
    public sealed class WorldSimulationState : IWorldSimulationState
    {
        private readonly IPlayerStateProvider _playerState;
        private readonly GameClock _gameClock;
        private readonly IPlayerTurnChoiceState _turnChoice;

        public WorldSimulationState(IPlayerStateProvider playerState, GameClock gameClock,
            IPlayerTurnChoiceState turnChoice)
        {
            _playerState = playerState;
            _gameClock = gameClock;
            _turnChoice = turnChoice;
        }

        public bool IsActive =>
            !_gameClock.IsPaused
            && _playerState.State == PlayerState.Moving
            && !_turnChoice.IsChoosingTurn;
    }
}
