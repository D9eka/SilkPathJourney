using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Npc.Core.NextSegmentProvider
{
    public interface IPathAware
    {
        public void SetFullPath(RoadPath path);
    }
}