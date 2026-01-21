using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Internal.Scripts.Npc.NextSegment;
using Internal.Scripts.Player.Input;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Player.UI.Arrow.Controller;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Player.NextSegment
{
    public class PlayerNextSegmentsProvider : INextSegmentProvider, IDestinationAware
    {
        private readonly PathHintsCreator _hints;
        private readonly IArrowsController _arrows;
        private readonly IPlayerChoiceInput _input;
        
        private string _targetNodeId;

        public PlayerNextSegmentsProvider(PathHintsCreator hints, IArrowsController arrows, IPlayerChoiceInput input)
        {
            _hints = hints;
            _arrows = arrows;
            _input = input;
        }

        public async UniTask<RoadPathSegment> ChooseNextAsync(List<RoadPathSegment> options, CancellationToken ct)
        {
            PathHints hints = _hints.GetPathHints(options[0].FromNodeId, _targetNodeId);
            if (hints == null)
            {
                throw new OperationCanceledException(ct);
            }
            _arrows.CreateArrows(options, hints);
        
            RoadPathSegment chosen = await _input.WaitForChoiceAsync(ct);
            _arrows.HideArrows();
            return chosen;
        }
        
        public void SetDestination(string destinationNodeId)
        {
            _targetNodeId = destinationNodeId;
        }
    }
}