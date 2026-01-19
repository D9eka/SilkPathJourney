using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Npc.Core.NextSegmentProvider
{
    public class NpcNextSegmentProvider : INextSegmentProvider, IPathAware
    {
        private int _segmentIndex;
        
        public RoadPath CurrentPath { get; private set; }
        
        public void SetFullPath(RoadPath path)
        {
            CurrentPath = path;
            _segmentIndex = 0;
        }
        
        public async UniTask<RoadPathSegment> ChooseNextAsync(IEnumerable<RoadPathSegment> options, CancellationToken cancelToken = default)
        {
            await Task.Yield();
            _segmentIndex++;
            if (_segmentIndex < CurrentPath.Segments.Count && options.Contains(CurrentPath.Segments[_segmentIndex]))
            {
                return CurrentPath.Segments[_segmentIndex];
            }
            throw new OperationCanceledException(cancelToken);
        }
    }
}