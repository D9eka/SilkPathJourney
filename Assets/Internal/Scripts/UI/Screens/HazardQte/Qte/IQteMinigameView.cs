using System;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Input;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public interface IQteMinigameView
    {
        event Action<bool> OnCompleted;
        bool DidPlayerSucceed();
        void Show(IHazardInputConfig config, InputRouter inputRouter);
        void Hide();
    }
}
