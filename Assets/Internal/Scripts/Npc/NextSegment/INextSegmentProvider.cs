using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Npc.NextSegment
{
    public interface INextSegmentProvider
    {
        UniTask<RoadPathSegment> ChooseNextAsync(
            List<RoadPathSegment> options, 
            CancellationToken cancelToken = default
            );
    }
}