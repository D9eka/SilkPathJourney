using System;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Minigames;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public interface IQteMinigameView
    {
        event Action<bool> OnCompleted;
        bool DidPlayerSucceed();
        void Show(IMinigameConfig config, IQteInput input);
        void Hide();
    }
}
