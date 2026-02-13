using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.UI.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.WorldLabel
{
    public class WorldLabelView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TextMeshProUGUI _nameText;
        private Image _icon;
        private LocalizedString _localizedString;
        private LocalizedString.ChangeHandler _locHandler;

        private ITooltipDataProvider _tooltipProvider;
        private TooltipService _tooltipService;
        private UnityEngine.Camera _camera;
        private WorldCanvasSettings _settings;
        private float _minCameraY;
        private float _maxCameraY;

        public TextMeshProUGUI NameText => _nameText;
        public Image IconImage => _icon;

        public void Initialize(TextMeshProUGUI nameText, Image icon,
            TooltipService tooltipService, UnityEngine.Camera camera,
            WorldCanvasSettings settings, CameraZoomerData zoomerData)
        {
            _nameText = nameText;
            _icon = icon;
            _tooltipService = tooltipService;
            _camera = camera;
            _settings = settings;

            _minCameraY = zoomerData.BaseYPosition +
                (zoomerData.MinValue - zoomerData.BaseSizeValue) / zoomerData.ScaleFactor;
            _maxCameraY = zoomerData.BaseYPosition +
                (zoomerData.MaxValue - zoomerData.BaseSizeValue) / zoomerData.ScaleFactor;

            HideIcon();
        }

        public void SetTooltipProvider(ITooltipDataProvider provider)
        {
            _tooltipProvider = provider;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltipProvider != null && _tooltipService != null)
            {
                Vector3 worldPos = transform.position;
                _tooltipService.ShowTooltipDelayed(_tooltipProvider, worldPos);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tooltipService != null)
            {
                _tooltipService.HideTooltip();
            }
        }

        public void SetText(string text)
        {
            ClearLocalization();
            _nameText.text = text;
        }

        public void SetLocalizedText(LocalizedString localized, string fallback)
        {
            ClearLocalization();

            if (localized == null || string.IsNullOrWhiteSpace(localized.TableReference.TableCollectionName))
            {
                _nameText.text = fallback;
                return;
            }

            _localizedString = localized;
            _locHandler = value =>
            {
                _nameText.text = string.IsNullOrWhiteSpace(value) ? fallback : value;
            };
            _localizedString.StringChanged += _locHandler;
        }

        public void SetIcon(Sprite sprite)
        {
            if (_icon == null) return;
            _icon.sprite = sprite;
            _icon.gameObject.SetActive(true);
        }

        public void HideIcon()
        {
            if (_icon == null) return;
            _icon.gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void ClearLocalization()
        {
            if (_localizedString != null && _locHandler != null)
            {
                _localizedString.StringChanged -= _locHandler;
            }
            _localizedString = null;
            _locHandler = null;
        }

        private void LateUpdate()
        {
            if (_camera == null || _settings == null) return;

            transform.rotation = _camera.transform.rotation;

            float t = Mathf.InverseLerp(_maxCameraY, _minCameraY, _camera.transform.position.y);
            float scale = Mathf.Lerp(_settings.MinLabelScale, _settings.MaxLabelScale, t);
            transform.localScale = Vector3.one * scale;
        }

        private void OnDestroy()
        {
            ClearLocalization();
        }
    }
}
