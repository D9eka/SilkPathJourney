using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Internal.Scripts.Player.NextSegment;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Npc.NextSegment
{
    public class NpcNextSegmentProvider : INextSegmentProvider, IDestinationAware
    {
        private readonly IRoadPathFinder _roadPathFinder;
        
        private string _destinationNodeId;
        private int _segmentIndex;
        
        public RoadPath CurrentPath { get; private set; }

        public NpcNextSegmentProvider(IRoadPathFinder roadPathFinder)
        {
            _roadPathFinder = roadPathFinder;
        }
        
        public async UniTask<RoadPathSegment> ChooseNextAsync(List<RoadPathSegment> options, CancellationToken cancelToken = default)
        {
            await Task.Yield();
            if (CurrentPath != null)
            {
                _segmentIndex++;
            }
            else
            {
                string startNodeId = options[0].FromNodeId;
                CurrentPath = _roadPathFinder.FindPath(startNodeId, _destinationNodeId);
            }
            if (_segmentIndex < CurrentPath.Segments.Count && options.Contains(CurrentPath.Segments[_segmentIndex]))
            {
                return CurrentPath.Segments[_segmentIndex];
            }
            throw new OperationCanceledException(cancelToken);
        }
        
        public void SetDestination(string destinationNodeId)
        {
            _destinationNodeId = destinationNodeId;
            CurrentPath = null;
            _segmentIndex = 0;
        }
    }
}