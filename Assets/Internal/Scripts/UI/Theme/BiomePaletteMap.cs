using System;
using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.UI.Theme
{
    [CreateAssetMenu(menuName = "SPJ/UI/Biome Palette Map", fileName = "BiomePaletteMap")]
    public class BiomePaletteMap : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public Biome Biome;
            public UiColorPalette Palette;
        }

        [SerializeField] private Entry[] _entries;
        [SerializeField] private UiColorPalette _fallback;

        public UiColorPalette Fallback => _fallback;

        public bool TryGetPalette(Biome biome, out UiColorPalette palette)
        {
            if (_entries != null)
            {
                foreach (Entry entry in _entries)
                {
                    if (entry.Biome == biome && entry.Palette != null)
                    {
                        palette = entry.Palette;
                        return true;
                    }
                }
            }

            palette = _fallback;
            return _fallback != null;
        }

#if UNITY_EDITOR
        public void ApplyImport(Entry[] entries, UiColorPalette fallback)
        {
            _entries = entries;
            _fallback = fallback;
        }
#endif
    }
}
