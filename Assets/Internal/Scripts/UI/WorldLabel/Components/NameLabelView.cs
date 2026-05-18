using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.Tooltip;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.WorldLabel.Components
{
    public class NameLabelView : MonoBehaviour
    {
        [SerializeField] private GameObject _nameContainer;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private WorldLabelIcon _icon;

        private TooltipService _tooltipService;
        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _locHandle;

        public TextMeshProUGUI NameText => _nameText;

        public void SetColorController(StaticColorController controller, Biome biome)
        {
            gameObject.InitializeColorBinders(colorController: controller, biome: biome);
            if (_icon != null && controller != null)
                _icon.SetTintColor(controller.GetColor(biome, ColorSlot.CityMarker));
        }

        public void Initialize(TooltipService tooltipService, LocalizationService localization)
        {
            _tooltipService = tooltipService;
            _localization = localization;
        }

        public void SetTooltipProvider(ITooltipDataProvider provider)
        {
            if (provider == null || _tooltipService == null || _icon == null) return;
            _icon.Initialize(_tooltipService, provider.GetTooltipTitle(), provider.GetTooltipDescription());
        }

        public void SetIconTooltip(string title, string description)
        {
            if (_icon == null || _tooltipService == null) return;
            _icon.Initialize(_tooltipService, title, description);
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
            _icon.SetIcon(sprite);
            _icon.gameObject.SetActive(true);
        }

        public void HideIcon()
        {
            if (_icon != null)
                _icon.gameObject.SetActive(false);
        }

        public void Show()
        {
            _nameContainer.SetActive(true);
        }

        public void Hide()
        {
            _nameContainer.SetActive(false);
        }

        private void OnDestroy()
        {
            _locHandle?.Dispose();
        }
    }
}
