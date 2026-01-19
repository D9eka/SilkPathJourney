using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Npc.Core.NextSegmentProvider
{
    public interface INextSegmentProvider
    {
        UniTask<RoadPathSegment> ChooseNextAsync(
            IEnumerable<RoadPathSegment> options, 
            CancellationToken cancelToken = default
            );
    }
}