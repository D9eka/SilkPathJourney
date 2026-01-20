using System;
public interface IInteractableObject
{
    event Action<IInteractableObject> OnClick;
    void TriggerClick();
    void TriggerHoverEnter();
    void TriggerHoverExit();
}