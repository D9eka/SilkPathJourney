namespace Internal.Scripts.Travel.Triggers
{
    public interface ITriggerAction
    {
        bool CanTrigger();
        void Trigger();
    }
}
