using System;
using Internal.Scripts.Economy.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Components
{
    [Serializable]
    public class ItemCategoryEntry
    {
        public ItemType Category;
        public Sprite Icon;
        public LocalizedString Name;
    }
}
