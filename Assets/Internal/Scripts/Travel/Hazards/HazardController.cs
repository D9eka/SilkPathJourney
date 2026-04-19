using Internal.Scripts.Events;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;

namespace Internal.Scripts.Travel.Hazards
{
    public sealed class HazardController
    {
        private readonly ScreenStackService _screenStackService;
        private readonly DayTracker _dayTracker;

        public bool IsActive => _screenStackService.IsOpen(ScreenId.HazardQte);

        public HazardController(ScreenStackService screenStackService, DayTracker dayTracker)
        {
            _screenStackService = screenStackService;
            _dayTracker = dayTracker;
        }

        public void Show(HazardData data)
        {
            if (_dayTracker.IsSkipping)
                return;
            if (_screenStackService.IsOpen(ScreenId.HazardQte))
                return;
            if (_screenStackService.IsOpen(ScreenId.Event))
                return;
            _screenStackService.TryOpen(ScreenId.HazardQte, data, out _);
        }

        public void ForceShow(HazardData data)
        {
            _screenStackService.TryOpen(ScreenId.HazardQte, data, out _);
        }
    }
}
