using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Core.Tabs
{
    [Serializable]
    public abstract class TabBinding : ITabBinding
    {
        [SerializeField] private TabButton _button;
        [SerializeField] private MonoBehaviour _view;
        [SerializeField] private LocalizedString _label;

        public abstract int TabId { get; }
        public TabButton Button => _button;
        public ITabView View => _view as ITabView;
        public LocalizedString Label => _label;
    }
}
