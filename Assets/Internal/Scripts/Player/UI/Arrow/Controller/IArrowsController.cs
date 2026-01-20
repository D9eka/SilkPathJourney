using System.Collections.Generic;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.Player.UI.Arrow.Controller
{
    public interface IArrowsController
    {
        List<ArrowView> GetAllArrows();
        void CreateArrows(IEnumerable<RoadPathSegment> allOptions, PathHints pathHints);
        void HideArrows();
    }
}