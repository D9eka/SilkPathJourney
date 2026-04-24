using System;
using System.Collections.Generic;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Minigames;
using Internal.Scripts.UI.Screens.HazardQte.Qte;
using Internal.Scripts.UI.Theme;
using Plugins.Zenject.Source.Main;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.UI.Screens.HazardQte
{
    public sealed class HazardMinigameSlot
    {
        private readonly Transform _container;
        private readonly LayoutElement _layoutElement;
        private readonly IInstantiator _instantiator;
        private readonly Dictionary<GameObject, GameObject> _cache = new();

        private IQteMinigameView _activeView;
        private GameObject _activeGo;
        private Action<bool> _onCompleted;

        public HazardMinigameSlot(Transform container, LayoutElement layoutElement, IInstantiator instantiator)
        {
            _container = container;
            _layoutElement = layoutElement;
            _instantiator = instantiator;
        }

        public void Show(GameObject prefab, IMinigameConfig config, IQteInput input, UiThemeService theme, Action<bool> onCompleted)
        {
            Hide();
            if (prefab == null) return;

            if (!_cache.TryGetValue(prefab, out _activeGo) || _activeGo == null)
            {
                _activeGo = _instantiator.InstantiatePrefab(prefab, _container);
                _activeGo.InitializeColorBinders(themeService: theme);
                _cache[prefab] = _activeGo;
            }

            _activeGo.SetActive(true);
            _activeView = _activeGo.GetComponent<IQteMinigameView>();
            if (_activeView == null)
            {
                _activeGo.SetActive(false);
                return;
            }

            _onCompleted = onCompleted;
            _activeView.OnCompleted += OnCompletedInternal;
            _activeView.Show(config, input);

            RebuildLayout();
        }

        public void Hide()
        {
            if (_activeView != null)
            {
                _activeView.OnCompleted -= OnCompletedInternal;
                _activeView.Hide();
                _activeView = null;
            }
            if (_activeGo != null)
            {
                _activeGo.SetActive(false);
                _activeGo = null;
            }
            _onCompleted = null;
        }

        public bool DidPlayerSucceed() => _activeView?.DidPlayerSucceed() ?? false;

        private void OnCompletedInternal(bool success) => _onCompleted?.Invoke(success);

        private void RebuildLayout()
        {
            var rect = (RectTransform)_activeGo.transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            float height = LayoutUtility.GetPreferredHeight(rect);
            if (height <= 0f) height = rect.rect.height;
            _layoutElement.preferredHeight = height;
        }
    }
}
