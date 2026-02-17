using Internal.Scripts.Road.Path;

namespace Internal.Scripts.UI.PathVisualization
{
    public interface IPathVisualizationService
    {
        void ShowPath(RoadPath path);
        void HidePath();
        void UpdatePath(RoadPath path);
    }
}
