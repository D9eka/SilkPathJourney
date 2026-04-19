using System.Collections.Generic;
using Internal.Scripts.UI.Components;

namespace Internal.Scripts.UI.Screens.Camp
{
    public sealed class CampViewState
    {
        public readonly IReadOnlyDictionary<ResourceType, float> ResourceValues;
        public readonly CampActionButtonState[] Actions;

        public CampViewState(IReadOnlyDictionary<ResourceType, float> resourceValues, CampActionButtonState[] actions)
        {
            ResourceValues = resourceValues;
            Actions = actions;
        }
    }
}
