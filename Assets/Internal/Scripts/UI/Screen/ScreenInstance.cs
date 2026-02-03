using Internal.Scripts.UI.Screen.Config;
using Internal.Scripts.UI.Screen.View;
using Internal.Scripts.UI.Screen.ViewModel;
using UnityEngine;

namespace Internal.Scripts.UI.Screen
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