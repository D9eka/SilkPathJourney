using UnityEngine.Localization;

namespace Internal.Scripts.Travel.Hazards.Input
{
    public interface IHazardInputConfig
    {
        HazardInputType InputType { get; }
        LocalizedString Hint { get; }
    }
}
