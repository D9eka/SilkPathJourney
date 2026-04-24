namespace Internal.Scripts.Travel.Triggers
{
    public interface ISegmentTriggerAction : ITriggerAction
    {
        int Weight { get; }
    }
}
