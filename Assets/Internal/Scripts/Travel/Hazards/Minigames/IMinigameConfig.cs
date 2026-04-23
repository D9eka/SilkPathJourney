using UnityEngine.Localization;

namespace Internal.Scripts.Travel.Hazards.Minigames
{
    public interface IMinigameConfig
    {
        LocalizedString Hint { get; }
    }
}
