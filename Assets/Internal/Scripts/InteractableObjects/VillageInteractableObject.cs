using System;
using Internal.Scripts.InteractableObjects.Abstraction;
using UnityEngine;

namespace Internal.Scripts.InteractableObjects
{
    public class VillageInteractableObject : InteractableObject
    {
        private Village _village;
        
        public Action<Village> Click;

        public void Initialize(Village village)
        {
            _village = village;
        }

        public override void OnClick()
        {
            base.OnClick();
            Click?.Invoke(_village);
        }
    }
}