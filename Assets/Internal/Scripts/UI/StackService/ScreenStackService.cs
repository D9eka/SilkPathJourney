using System.Collections.Generic;
using Internal.Scripts.UI.Factory;
using Internal.Scripts.UI.Screens.Config;
using Internal.Scripts.UI.Screens.Core;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.UI.StackService
{
    public sealed class ScreenStackService : IInitializable
    {
        private readonly UIScreenRoots _roots;
        private readonly ScreenCatalog _catalog;
        private readonly IScreenViewModelFactory _viewModelFactory;
        private readonly StaticColorController _colorController;
        private readonly UiThemeService _themeService;
        private readonly ScreenId _initialScreenId;
        private readonly List<ScreenInstance> _stack = new();
        private readonly Dictionary<ScreenId, ScreenInstance> _instances = new();

        public ScreenId TopId => _stack.Count > 0 ? _stack[^1].Id : ScreenId.None;

        public ScreenStackService(UIScreenRoots roots, ScreenCatalog catalog,
            IScreenViewModelFactory viewModelFactory, StaticColorController colorController,
            UiThemeService themeService, ScreenId initialScreenId)
        {
            _roots = roots;
            _catalog = catalog;
            _viewModelFactory = viewModelFactory;
            _colorController = colorController;
            _themeService = themeService;
            _initialScreenId = initialScreenId;
        }

        public void Initialize()
        {
            if (TryOpen(_initialScreenId, out ScreenOpenResult result))
            {
                return;
            }
            Debug.LogWarning($"[SPJ] Cannot open initial screen: {result}");
        }

        public bool IsOpen(ScreenId id)
        {
            return _stack.Exists(instance => instance.Id == id);
        }

        public bool TryOpen(ScreenId id, object args, out ScreenOpenResult result)
        {
            if (!_catalog.TryGet(id, out ScreenConfig config) || config == null)
            {
                result = ScreenOpenResult.MissingConfig;
                return false;
            }

            if (config.Prefab == null)
            {
                result = ScreenOpenResult.MissingPrefab;
                return false;
            }

            if (IsOpen(id))
            {
                result = ScreenOpenResult.AlreadyOpen;
                return false;
            }

            if (IsBlockedByExclusive(id))
            {
                result = ScreenOpenResult.BlockedByExclusive;
                return false;
            }

            ScreenInstance previousTop = _stack.Count > 0 ? _stack[^1] : null;

            if (!_instances.TryGetValue(id, out ScreenInstance instance))
            {
                Transform parent = _roots != null ? _roots.ScreensRoot : null;
                if (parent == null)
                {
                    result = ScreenOpenResult.MissingRoot;
                    return false;
                }

                GameObject instanceGo = Object.Instantiate(config.Prefab, parent);

                SetupColors(instanceGo);

                ScreenViewBase view = FindView(instanceGo);
                if (view == null)
                {
                    Object.Destroy(instanceGo);
                    result = ScreenOpenResult.MissingView;
                    return false;
                }

                ApplySortingOrder(instanceGo, config.SortOrder);

                ScreenViewModelBase viewModel = _viewModelFactory?.Create(id, view);
                if (viewModel == null)
                {
                    Object.Destroy(instanceGo);
                    result = ScreenOpenResult.MissingViewModel;
                    return false;
                }

                instance = new ScreenInstance(config.Id, config, instanceGo, view, viewModel);
                if (view is IScreenViewModelBinder binder)
                    binder.BindViewModel(viewModel);
                view.CloseRequested += () => Close(id);
                _instances.Add(id, instance);
            }

            _stack.Add(instance);
            previousTop?.ViewModel?.OnFocusLost();
            if (config.HidesBelowScreen && previousTop != null)
                previousTop.View?.Hide();
            instance.View.Show();
            instance.ViewModel.Open(args);
            instance.ViewModel.OnFocusGained();

            UpdateOverlays();

            result = ScreenOpenResult.Success;
            return true;
        }

        public bool TryOpen(ScreenId id, out ScreenOpenResult result)
        {
            return TryOpen(id, null, out result);
        }

        public void CloseTop()
        {
            if (_stack.Count == 0)
                return;

            CloseAtIndex(_stack.Count - 1);
        }

        public void Close(ScreenId id)
        {
            int index = _stack.FindLastIndex(instance => instance.Id == id);
            if (index < 0)
                return;

            CloseAtIndex(index);
        }

        public void CloseAllAbove(ScreenId keepId)
        {
            if (!IsOpen(keepId))
                return;

            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if (_stack[i].Id == keepId)
                    break;
                CloseAtIndex(i);
            }
        }

        private void CloseAtIndex(int index)
        {
            ScreenInstance instance = _stack[index];
            bool wasTop = index == _stack.Count - 1;

            instance.ViewModel?.Close();
            instance.View?.Hide();

            _stack.RemoveAt(index);

            if (wasTop && _stack.Count > 0)
            {
                _stack[^1].ViewModel?.OnFocusGained();
                if (instance.Config != null && instance.Config.HidesBelowScreen)
                    _stack[^1].View?.Show();
            }

            UpdateOverlays();
        }

        private bool IsBlockedByExclusive(ScreenId openingId)
        {
            foreach (ScreenInstance instance in _stack)
            {
                ScreenConfig config = instance.Config;
                if (config == null)
                    continue;

                if (config.IsExclusive)
                {
                    if (config.AllowedToOpenWhileExclusive == null ||
                        !config.AllowedToOpenWhileExclusive.Contains(openingId))
                    {
                        return true;
                    }
                }
            }

            if (_catalog.TryGet(openingId, out ScreenConfig openingConfig) && openingConfig != null)
            {
                if (openingConfig.IsExclusive && _stack.Count > 0)
                    return true;
            }

            return false;
        }

        private void UpdateOverlays()
        {
            if (_roots == null)
                return;

            bool worldBlocked = false;
            bool uiBlocked = false;

            foreach (ScreenInstance instance in _stack)
            {
                ScreenConfig config = instance.Config;
                if (config == null)
                    continue;

                if (config.BlocksWorldInput)
                    worldBlocked = true;

                if (instance == _stack[^1] && config.BlocksUIUnderneath)
                    uiBlocked = true;
            }

            _roots.SetWorldBlockerVisible(worldBlocked);
            _roots.SetUiBlockerVisible(uiBlocked);
        }

        private static void ApplySortingOrder(GameObject root, int sortOrder)
        {
            if (root == null)
                return;

            Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                Transform parent = canvas.transform.parent;
                if (parent != null && parent.GetComponentInParent<Canvas>() != null)
                    continue;

                canvas.overrideSorting = true;
                canvas.sortingOrder = sortOrder;
            }
        }
        
        private void SetupColors(GameObject instanceGo)
        {
            foreach (UiColorBinder colorBinder in 
                instanceGo.GetComponentsInChildren<UiColorBinder>(true))
            {
                colorBinder.Initialize(_themeService);
            }
            foreach (UiStaticColorBinder staticColorBinder in 
                instanceGo.GetComponentsInChildren<UiStaticColorBinder>(true))
            {
                staticColorBinder.Initialize(_colorController);
            }
        }

        private static ScreenViewBase FindView(GameObject root)
        {
            if (root == null)
                return null;

            if (root.TryGetComponent(out ScreenViewBase view))
                return view;

            foreach (ScreenViewBase child in root.GetComponentsInChildren<ScreenViewBase>(true))
                return child;

            return null;
        }
    }
}
