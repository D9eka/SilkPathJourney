using Internal.Scripts.UI.Screens.Core.Tabs;
using System;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Legacy
{
    [Serializable]
    public sealed class LegacyTabBinding : TabBinding
    {
        [SerializeField] private LegacyTab _tab;

        public LegacyTab Tab => _tab;
        public override int TabId => (int)_tab;
    }
}
