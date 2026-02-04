using System.Collections.Generic;
using Internal.Scripts.Player.Path;
using Internal.Scripts.Road.Path;

namespace Internal.Scripts.UI.Arrow.Controller
{
    public interface IArrowsController
    {
        List<ArrowView> GetAllArrows();
        void CreateArrows(List<RoadPathSegment> allOptions, PathHints pathHints);
        void HideArrows();
    }
}