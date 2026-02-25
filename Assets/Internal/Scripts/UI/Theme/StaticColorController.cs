using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.UI.Theme
{
    public class StaticColorController
    {
        private readonly BiomePaletteMap _paletteMap;

        public StaticColorController(BiomePaletteMap paletteMap) => _paletteMap = paletteMap;

        public Color GetColor(Biome biome, ColorSlot slot)
        {
            return _paletteMap.TryGetPalette(biome, out var p)
                ? p.GetColor(slot)
                : Color.magenta;
        }

        public void Register(UiStaticColorBinder binder)
        {
            binder.SetColor(GetColor(binder.Biome, binder.Slot));
        }
    }
}
