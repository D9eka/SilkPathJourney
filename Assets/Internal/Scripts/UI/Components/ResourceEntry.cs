using System;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    [Serializable]
    public class ResourceEntry
    {
        public ResourceType Type;
        public Sprite Icon;
        public bool IncreaseIsPositive = true;
    }
}
