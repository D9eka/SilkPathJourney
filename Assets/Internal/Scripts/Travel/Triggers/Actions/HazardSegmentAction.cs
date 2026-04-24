using Internal.Scripts.Config;
using Internal.Scripts.Travel.Hazards;

namespace Internal.Scripts.Travel.Triggers.Actions
{
    public sealed class HazardSegmentAction : ISegmentTriggerAction
    {
        private readonly HazardSelector _selector;
        private readonly HazardController _controller;
        private readonly GameBalanceConfig _balance;

        public int Weight => _balance.SegmentWeights.Hazard;

        public HazardSegmentAction(
            HazardSelector selector,
            HazardController controller,
            GameBalanceConfig balance)
        {
            _selector = selector;
            _controller = controller;
            _balance = balance;
        }

        public bool CanTrigger() => !_controller.IsActive;

        public void Trigger()
        {
            HazardData hazard = _selector.SelectHazard();
            if (hazard != null)
                _controller.Show(hazard);
        }
    }
}
