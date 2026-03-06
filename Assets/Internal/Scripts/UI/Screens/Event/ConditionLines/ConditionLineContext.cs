using Internal.Scripts.Events.Data;

namespace Internal.Scripts.UI.Screens.Event.ConditionLines
{
    public readonly struct ConditionLineContext
    {
        public readonly EventData EventData;
        public readonly EventChoice Choice;
        public readonly int OriginalChoiceIndex;

        public ConditionLineContext(EventData eventData, EventChoice choice, int originalChoiceIndex)
        {
            EventData = eventData;
            Choice = choice;
            OriginalChoiceIndex = originalChoiceIndex;
        }
    }
}
