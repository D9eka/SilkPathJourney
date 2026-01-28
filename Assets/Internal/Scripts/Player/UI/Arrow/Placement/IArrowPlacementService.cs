using System.Collections.Generic;

namespace Internal.Scripts.Player.UI.Arrow.Placement
{
    public interface IArrowPlacementService
    {
        void PlaceArrows(List<ArrowData> arrowDataList);
        
        void HideArrows();
        
        List<ArrowView> GetAllArrows();
        
        void ClearArrows();
    }
}