using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Core
{
    public sealed class ScreenInstance
    {
        public ScreenId Id { get; }
        public ScreenConfig Config { get; }
        public GameObject Root { get; }
        public ScreenViewBase View { get; }
        public ScreenViewModelBase ViewModel { get; }

        public ScreenInstance(ScreenId id, ScreenConfig config, GameObject root, ScreenViewBase view, ScreenViewModelBase viewModel)
        {
            Id = id;
            Config = config;
            Root = root;
            View = view;
            ViewModel = viewModel;
        }
    }
}