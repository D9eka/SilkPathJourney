using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Internal.Scripts.Player.Input;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Player.UI.Arrow.Controller;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Npc.Core.NextSegmentProvider
{
    public class PlayerNextSegmentsProvider : INextSegmentProvider, ITargetAware
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

        public async UniTask<RoadPathSegment> ChooseNextAsync(IEnumerable<RoadPathSegment> options, CancellationToken ct)
        {
            List<RoadPathSegment> opts = options.ToList();
            PathHints hints = _hints.GetPathHints(opts.First().FromNodeId, _targetNodeId);
            if (hints == null)
            {
                throw new OperationCanceledException(ct);
            }
            _arrows.CreateArrows(opts, hints);
        
            RoadPathSegment chosen = await _input.WaitForChoiceAsync(ct);
            _arrows.HideArrows();
            return chosen;
        }
        
        public void SetTargetNodeId(string targetNodeId)
        {
            _targetNodeId = targetNodeId;
        }
    }
}