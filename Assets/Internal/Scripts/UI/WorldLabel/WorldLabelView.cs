using Internal.Scripts.Economy.Generated;
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

        private LocalizedString _localizedString;
        private LocalizedString.ChangeHandler _locHandler;

        private TooltipService _tooltipService;

        public TextMeshProUGUI NameText => _nameText;

        public void SetColorController(StaticColorController controller, Biome biome)
        {
            foreach (var binder in GetComponentsInChildren<UiStaticColorBinder>(true))
            {
                binder.Initialize(controller);
                binder.SetBiome(biome);
            }
        }

        public void Initialize(TooltipService tooltipService)
        {
            _tooltipService = tooltipService;
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

        private void ClearLocalization()
        {
            if (_localizedString != null && _locHandler != null)
                _localizedString.StringChanged -= _locHandler;
            _localizedString = null;
            _locHandler = null;
        }

        private void OnDestroy()
        {
            ClearLocalization();
        }
    }
}
