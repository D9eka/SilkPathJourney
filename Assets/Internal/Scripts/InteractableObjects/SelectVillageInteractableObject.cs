using System;
using Internal.Scripts.InteractableObjects.Abstraction;

namespace Internal.Scripts.InteractableObjects
{
    public class SelectVillageInteractableObject : InteractableObject
    {
        public Action Click;

        public override void OnClick()
        {
            Click?.Invoke();
            base.OnClick();
        }
    }
}