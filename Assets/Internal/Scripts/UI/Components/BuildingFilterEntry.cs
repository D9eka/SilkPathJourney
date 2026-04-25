using System;
using Internal.Scripts.Economy.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Components
{
    [Serializable]
    public class BuildingFilterEntry
    {
        public BuildingId Building;
        public Sprite Icon;
        public LocalizedString Name;
    }
}
