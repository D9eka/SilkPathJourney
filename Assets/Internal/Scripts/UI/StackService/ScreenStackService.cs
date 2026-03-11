using System;
using System.Collections.Generic;
using Internal.Scripts.Camera.Move;
using Internal.Scripts.UI.Factory;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Config;
using Internal.Scripts.UI.Screens.Core;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.Screens.Core.View;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Internal.Scripts.UI.StackService
{
    public sealed class ScreenStackService : IInitializable
    {
        private readonly UIScreenRoots _roots;
        private readonly ScreenCatalog _catalog;
        private readonly IScreenViewModelFactory _viewModelFactory;
        private readonly LocalizationService _localizationService;
        private readonly StaticColorController _colorController;
        private readonly UiThemeService _themeService;
        private readonly ScreenId _initialScreenId;
        private readonly List<ScreenInstance> _stack = new();
        private readonly Dictionary<ScreenId, ScreenInstance> _instances = new();

        [InjectOptional] private ICameraMover _cameraMover;

        public ScreenId TopId => _stack.Count > 0 ? _stack[^1].Id : ScreenId.None;

        public event Action<ScreenId> OnScreenClosed;

        public ScreenStackService(UIScreenRoots roots, ScreenCatalog catalog,
            IScreenViewModelFactory viewModelFactory, LocalizationService localizationService,
            StaticColorController colorController,
            UiThemeService themeService, ScreenId initialScreenId)
        {
            _roots = roots;
            _catalog = catalog;
            _viewModelFactory = viewModelFactory;
            _localizationService = localizationService;
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
            if (!ValidateOpenRequest(id, out ScreenConfig config, out result))
                return false;

            ScreenInstance previousTop = _stack.Count > 0 ? _stack[^1] : null;

            if (!_instances.TryGetValue(id, out ScreenInstance instance))
            {
                instance = InstantiateScreen(id, config, out result);
                if (instance == null)
                    return false;
            }

            _stack.Add(instance);
            previousTop?.ViewModel?.OnFocusLost();
            if (config.HidesBelowScreen && previousTop != null)
                previousTop.View?.Hide();
            instance.View.Show();
            instance.ViewModel.Open(args);
            instance.ViewModel.OnFocusGained();

            UpdateSortingOrders();
            UpdateOverlays();
            _cameraMover?.ResetMovement();

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

        private bool ValidateOpenRequest(ScreenId id, out ScreenConfig config, out ScreenOpenResult result)
        {
            config = null;

            if (!_catalog.TryGet(id, out config) || config == null)
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

            result = ScreenOpenResult.Success;
            return true;
        }

        private ScreenInstance InstantiateScreen(ScreenId id, ScreenConfig config, out ScreenOpenResult result)
        {
            Transform parent = _roots != null ? _roots.ScreensRoot : null;
            if (parent == null)
            {
                result = ScreenOpenResult.MissingRoot;
                return null;
            }

            GameObject instanceGo = Object.Instantiate(config.Prefab, parent);

            instanceGo.InitializeColorBinders(_themeService, _colorController);
            SetupLocalization(instanceGo);

            ScreenViewBase view = FindView(instanceGo);
            if (view == null)
            {
                Object.Destroy(instanceGo);
                result = ScreenOpenResult.MissingView;
                return null;
            }

            ScreenViewModelBase viewModel = _viewModelFactory?.Create(id, view);
            if (viewModel == null)
            {
                Object.Destroy(instanceGo);
                result = ScreenOpenResult.MissingViewModel;
                return null;
            }

            ScreenInstance instance = new ScreenInstance(config.Id, config, instanceGo, view, viewModel);
            if (view is IScreenViewModelBinder binder)
                binder.BindViewModel(viewModel);
            view.CloseRequested += () => Close(id);
            _instances.Add(id, instance);

            result = ScreenOpenResult.Success;
            return instance;
        }

        private void CloseAtIndex(int index)
        {
            ScreenInstance instance = _stack[index];
            bool wasTop = index == _stack.Count - 1;
            ScreenId closedId = instance.Id;

            instance.ViewModel?.Close();
            instance.View?.Hide();

            _stack.RemoveAt(index);

            if (wasTop && _stack.Count > 0)
            {
                _stack[^1].ViewModel?.OnFocusGained();
                if (instance.Config != null && instance.Config.HidesBelowScreen)
                    _stack[^1].View?.Show();
            }

            UpdateSortingOrders();
            UpdateOverlays();
            OnScreenClosed?.Invoke(closedId);
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

        private void UpdateSortingOrders()
        {
            for (int i = 0; i < _stack.Count; i++)
                ApplySortingOrder(_stack[i].Root, i * 2);
        }

        private void UpdateOverlays()
        {
            if (_roots == null)
                return;

            if (_stack.Count > 0)
            {
                ScreenInstance top = _stack[^1];
                bool showDim = top.Config != null && top.Config.ShowsDimOverlay;
                int overlayOrder = (_stack.Count - 1) * 2 - 1;
                _roots.SetDimOverlayVisible(showDim, overlayOrder);
            }
            else
            {
                _roots.SetDimOverlayVisible(false);
            }
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

                canvas.sortingOrder = sortOrder;
            }
        }
        
        private void SetupLocalization(GameObject instanceGo)
        {
            foreach (ILocalizationConsumer consumer in
                instanceGo.GetComponentsInChildren<ILocalizationConsumer>(true))
            {
                consumer.SetLocalization(_localizationService);
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
