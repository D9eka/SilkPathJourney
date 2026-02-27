using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.WorldLabel
{
    public class WorldLabelView : MonoBehaviour
    {
        [SerializeField] private GameObject _cityNameContainer;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _icon;
        [SerializeField] private RectTransform _iconsContainer;
        [SerializeField] private Image _iconTemplate;

        private TooltipService _tooltipService;
        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _locHandle;

        public TextMeshProUGUI NameText => _nameText;

        public void SetColorController(StaticColorController controller, Biome biome)
        {
            foreach (var binder in GetComponentsInChildren<UiStaticColorBinder>(true))
            {
                binder.Initialize(controller);
                binder.SetBiome(biome);
            }
        }

        public void Initialize(TooltipService tooltipService, LocalizationService localization)
        {
            _tooltipService = tooltipService;
            _localization = localization;
        }

        public void SetTooltipProvider(ITooltipDataProvider provider)
        {
            if (provider == null || _tooltipService == null) return;
            var icon = _nameText.gameObject.AddComponent<WorldLabelIcon>();
            icon.Initialize(_tooltipService, provider.GetTooltipTitle(), provider.GetTooltipDescription());
        }

        public void SetIconTooltip(string title, string description)
        {
            if (_icon == null || _tooltipService == null) return;
            var iconComp = _icon.gameObject.AddComponent<WorldLabelIcon>();
            iconComp.Initialize(_tooltipService, title, description);
        }

        public void SetText(string text)
        {
            _locHandle?.Dispose();
            _locHandle = null;
            _nameText.text = text;
        }

        public void SetLocalizedText(LocalizedString localized, string fallback)
        {
            _locHandle?.Dispose();
            _locHandle = null;

            if (_localization == null || localized == null
                || string.IsNullOrWhiteSpace(localized.TableReference.TableCollectionName))
            {
                _nameText.text = fallback;
                return;
            }

            _locHandle = _localization.BindText(_nameText, localized, "WorldLabel");
        }

        public void SetIcon(Sprite sprite)
        {
            if (_icon == null) return;
            _icon.sprite = sprite;
            _icon.gameObject.SetActive(true);
        }

        public void HideIcon()
        {
            if (_icon != null)
                _icon.gameObject.SetActive(false);
        }

        public void AddIcon(Sprite icon, string name, string description)
        {
            if (_iconTemplate == null || _iconsContainer == null) return;

            GameObject iconGo = Instantiate(_iconTemplate.gameObject, _iconsContainer);
            iconGo.SetActive(true);

            Image image = iconGo.GetComponent<Image>();
            if (image != null)
                image.sprite = icon;

            WorldLabelIcon labelIcon = iconGo.AddComponent<WorldLabelIcon>();
            labelIcon.Initialize(_tooltipService, name, description);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _locHandle?.Dispose();
        }
    }
}
