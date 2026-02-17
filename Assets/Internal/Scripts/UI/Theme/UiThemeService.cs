using System;

namespace Internal.Scripts.UI.Theme
{
    public class UiThemeService
    {
        public UiColorPalette ActivePalette { get; private set; }

        public event Action<UiColorPalette> PaletteChanged;

        public UiThemeService(UiColorPalette defaultPalette)
        {
            ActivePalette = defaultPalette;
        }

        public void SetPalette(UiColorPalette palette)
        {
            if (palette == null || palette == ActivePalette)
                return;

            ActivePalette = palette;
            PaletteChanged?.Invoke(palette);
        }

        public UnityEngine.Color GetColor(ColorSlot slot)
        {
            return ActivePalette != null ? ActivePalette.GetColor(slot) : UnityEngine.Color.magenta;
        }
    }
}
