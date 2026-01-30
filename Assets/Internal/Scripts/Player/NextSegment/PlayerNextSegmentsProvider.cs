using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Internal.Scripts.Npc.NextSegment;
using Internal.Scripts.Player.Input;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Road.Path;
using Internal.Scripts.UI.Arrow.Controller;

namespace Internal.Scripts.Player.NextSegment
{
    public class PlayerNextSegmentsProvider : INextSegmentProvider, IDestinationAware, IPlayerTurnChoiceState
    {
        private readonly PathHintsCreator _hints;
        private readonly IArrowsController _arrows;
        private readonly IPlayerChoiceInput _input;
        
        private string _targetNodeId;
        private bool _isChoosingTurn;
        private string _currentTurnNodeId;

        public bool IsChoosingTurn => _isChoosingTurn;
        public string CurrentTurnNodeId => _currentTurnNodeId ?? string.Empty;

        public PlayerNextSegmentsProvider(PathHintsCreator hints, IArrowsController arrows, IPlayerChoiceInput input)
        {
            _hints = hints;
            _arrows = arrows;
            _input = input;
        }

        public async UniTask<RoadPathSegment> ChooseNextAsync(List<RoadPathSegment> options, CancellationToken ct)
        {
            _isChoosingTurn = true;
            _currentTurnNodeId = options.Count > 0 ? options[0].FromNodeId : string.Empty;
            PathHints hints = _hints.GetPathHints(options[0].FromNodeId, _targetNodeId);
            if (hints == null)
            {
                _isChoosingTurn = false;
                _currentTurnNodeId = string.Empty;
                throw new OperationCanceledException(ct);
            }
            _arrows.CreateArrows(options, hints);

            try
            {
                RoadPathSegment chosen = await _input.WaitForChoiceAsync(ct);
                return chosen;
            }
            finally
            {
                _arrows.HideArrows();
                _isChoosingTurn = false;
                _currentTurnNodeId = string.Empty;
            }
        }
        
        public void SetDestination(string destinationNodeId)
        {
            _targetNodeId = destinationNodeId;
        }
    }
}
