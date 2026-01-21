using System;
namespace Internal.Scripts.InteractableObjects
{
    public interface IInteractableObject
    {
        event Action<IInteractableObject> OnClick;
        void TriggerClick();
        void TriggerHoverEnter();
        void TriggerHoverExit();
    }
}