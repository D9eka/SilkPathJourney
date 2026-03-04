using Internal.Scripts.Events.Generated;
using Internal.Scripts.UI.Components;

namespace Internal.Scripts.Events.Outcomes
{
    public interface IResourceOutcomeApplier : IOutcomeApplier
    {
        ResourceType? GetAffectedResource(EventOutcomeType type);
        bool CanAfford(EventOutcomeType type, float netValue);
    }
}
