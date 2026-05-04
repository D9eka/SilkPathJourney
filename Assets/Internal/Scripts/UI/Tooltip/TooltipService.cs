using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zenject;

namespace Internal.Scripts.UI.Tooltip
{
    public class TooltipService : IInitializable, IDisposable
    {
        private const float SHOW_DELAY = 0.3f;
        private static readonly Vector2 CURSOR_OFFSET = new Vector2(20f, -20f);
        private static readonly Vector2 WORLD_LABEL_OFFSET = new Vector2(20f, 20f);

        private UnityEngine.Camera _mainCamera;
        private readonly TooltipView _tooltipView;

        private Sequence _showSequence;
        private ITooltipDataProvider _currentProvider;

        public TooltipService(UnityEngine.Camera mainCamera, TooltipView tooltipView)
        {
            _mainCamera = mainCamera;
            _tooltipView = tooltipView;
        }

        public void Initialize()
        {
            RefreshCamera();
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        public void Dispose()
        {
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }

        private void HandleActiveSceneChanged(Scene previous, Scene next)
        {
            HideTooltip();
            RefreshCamera();
        }

        private void RefreshCamera() => _mainCamera = UnityEngine.Camera.main;
        
        public void ShowTooltip(ITooltipDataProvider dataProvider, Vector3? worldPosition = null)
        {
            if (dataProvider == null)
            {
                Debug.LogWarning("[TooltipService] Attempted to show tooltip with null data provider");
                return;
            }

            _showSequence?.Kill();
            _currentProvider = dataProvider;

            string title = dataProvider.GetTooltipTitle();
            string description = dataProvider.GetTooltipDescription();

            _tooltipView.Show(title, description);

            Vector2 screenPos = CalculatePosition(worldPosition);
            _tooltipView.SetPosition(screenPos);
        }
        
        public void ShowTooltipDelayed(ITooltipDataProvider dataProvider, Vector3? worldPosition = null)
        {
            if (dataProvider == null)
            {
                Debug.LogWarning("[TooltipService] Attempted to show delayed tooltip with null data provider");
                return;
            }

            _showSequence?.Kill();
            _currentProvider = dataProvider;

            _showSequence = DOTween.Sequence()
                .AppendInterval(SHOW_DELAY)
                .OnComplete(() => ShowTooltip(dataProvider, worldPosition));
        }
        
        public void HideTooltip()
        {
            _showSequence?.Kill();
            _currentProvider = null;
            _tooltipView.Hide();
        }

        private Vector2 CalculatePosition(Vector3? worldPosition)
        {
            Vector2 screenPos;

            if (worldPosition.HasValue)
            {
                if (_mainCamera == null)
                    RefreshCamera();

                if (_mainCamera != null)
                {
                    screenPos = _mainCamera.WorldToScreenPoint(worldPosition.Value);
                    screenPos += WORLD_LABEL_OFFSET;
                }
                else if (Mouse.current != null)
                {
                    screenPos = Mouse.current.position.ReadValue();
                    screenPos += CURSOR_OFFSET;
                }
                else
                {
                    screenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
                }
            }
            else
            {
                if (Mouse.current != null)
                {
                    screenPos = Mouse.current.position.ReadValue();
                    screenPos += CURSOR_OFFSET;
                }
                else
                {
                    screenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
                }
            }

            return screenPos;
        }
    }
}
