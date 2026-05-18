using System;
namespace Internal.Scripts.InteractableObjects
{
    public interface IInteractableObject
    {
        int InteractionPriority { get; }
        event Action<IInteractableObject> OnClick;
        void TriggerClick();
        void TriggerHoverEnter();
        void TriggerHoverExit();
    }
}