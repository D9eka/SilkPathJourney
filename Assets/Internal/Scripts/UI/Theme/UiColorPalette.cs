using UnityEngine;

namespace Internal.Scripts.UI.Theme
{
    [CreateAssetMenu(menuName = "SPJ/UI/Color Palette", fileName = "UiColorPalette")]
    public class UiColorPalette : ScriptableObject
    {
        [Header("Window")]
        [field: SerializeField] public Color WindowBackground { get; private set; }
        [field: SerializeField] public Color WindowContainer { get; private set; }
        [field: SerializeField] public Color WindowElement { get; private set; }
        [field: SerializeField] public Color WindowText { get; private set; }
        [field: SerializeField] public Color WindowElementText { get; private set; }
        [field: SerializeField] public Color WindowContainerText { get; private set; }

        [Header("HUD")]
        [field: SerializeField] public Color HudElementBackground { get; private set; }
        [field: SerializeField] public Color HudElementText { get; private set; }

        [Header("Overlay")]
        [field: SerializeField] public Color OverlayColor { get; private set; }

        [Header("Icons")]
        [field: SerializeField] public Color IconPrimary { get; private set; }
        [field: SerializeField] public Color IconSecondary { get; private set; }

        [Header("Controls")]
        [field: SerializeField] public Color TimeButtonBackground { get; private set; }
        [field: SerializeField] public Color SliderBackground { get; private set; }
        [field: SerializeField] public Color SliderFillPositive { get; private set; }
        [field: SerializeField] public Color SliderFillNegative { get; private set; }
        [field: SerializeField] public Color ButtonSelectedBorder { get; private set; }

        public Color GetColor(ColorSlot slot) => slot switch
        {
            ColorSlot.WindowBackground => WindowBackground,
            ColorSlot.WindowContainer => WindowContainer,
            ColorSlot.WindowElement => WindowElement,
            ColorSlot.WindowText => WindowText,
            ColorSlot.WindowElementText => WindowElementText,
            ColorSlot.WindowContainerText => WindowContainerText,
            ColorSlot.HudElementBackground => HudElementBackground,
            ColorSlot.HudElementText => HudElementText,
            ColorSlot.OverlayColor => OverlayColor,
            ColorSlot.IconPrimary => IconPrimary,
            ColorSlot.IconSecondary => IconSecondary,
            ColorSlot.TimeButtonBackground => TimeButtonBackground,
            ColorSlot.SliderBackground => SliderBackground,
            ColorSlot.SliderFillPositive => SliderFillPositive,
            ColorSlot.SliderFillNegative => SliderFillNegative,
            ColorSlot.ButtonSelectedBorder => ButtonSelectedBorder,
            _ => Color.magenta,
        };

#if UNITY_EDITOR
        public void ApplyImport(
            Color windowBackground,
            Color windowContainer,
            Color windowElement,
            Color windowText,
            Color windowElementText,
            Color windowContainerText,
            Color hudElementBackground,
            Color hudElementText,
            Color overlayColor,
            Color iconPrimary,
            Color iconSecondary,
            Color timeButtonBackground,
            Color sliderBackground,
            Color sliderFillPositive,
            Color sliderFillNegative,
            Color buttonSelectedBorder)
        {
            WindowBackground = windowBackground;
            WindowContainer = windowContainer;
            WindowElement = windowElement;
            WindowText = windowText;
            WindowElementText = windowElementText;
            WindowContainerText = windowContainerText;
            HudElementBackground = hudElementBackground;
            HudElementText = hudElementText;
            OverlayColor = overlayColor;
            IconPrimary = iconPrimary;
            IconSecondary = iconSecondary;
            TimeButtonBackground = timeButtonBackground;
            SliderBackground = sliderBackground;
            SliderFillPositive = sliderFillPositive;
            SliderFillNegative = sliderFillNegative;
            ButtonSelectedBorder = buttonSelectedBorder;
        }
#endif
    }
}
