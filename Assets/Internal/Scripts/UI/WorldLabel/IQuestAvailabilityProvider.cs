using R3;

namespace Internal.Scripts.UI.WorldLabel
{
    public interface IQuestAvailabilityProvider
    {
        ReadOnlyReactiveProperty<bool> HasAvailableQuest { get; }
    }
}
