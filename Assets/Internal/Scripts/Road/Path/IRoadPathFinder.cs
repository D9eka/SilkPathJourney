namespace Internal.Scripts.Road.Path
{
    public interface IRoadPathFinder
    {
        RoadPath FindPath(string startNodeId, string targetNodeId);
    }
}
