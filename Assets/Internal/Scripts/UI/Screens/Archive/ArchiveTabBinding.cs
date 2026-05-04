using Internal.Scripts.UI.Screens.Core.Tabs;
using System;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Archive
{
    [Serializable]
    public sealed class ArchiveTabBinding : TabBinding
    {
        [SerializeField] private ArchiveTab _tab;

        public ArchiveTab Tab => _tab;
        public override int TabId => (int)_tab;
    }
}
