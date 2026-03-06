using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.UI.Theme
{
    public static class ColorBinderExtensions
    {
        public static void InitializeColorBinders(this GameObject go,
            UiThemeService themeService = null,
            StaticColorController colorController = null,
            Biome? biome = null)
        {
            if (themeService != null)
                foreach (var b in go.GetComponentsInChildren<UiColorBinder>(true))
                    b.Initialize(themeService);

            if (colorController != null)
                foreach (var b in go.GetComponentsInChildren<UiStaticColorBinder>(true))
                {
                    b.Initialize(colorController);
                    if (biome.HasValue)
                        b.SetBiome(biome.Value);
                }
        }
    }
}
