using System;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Player;
using Zenject;

namespace Internal.Scripts.UI.Theme
{
    public class BiomeThemeController : IInitializable, IDisposable
    {
        private readonly IPlayerStateEvents _playerEvents;
        private readonly ICityNodeResolver _cityResolver;
        private readonly UiThemeService _themeService;
        private readonly BiomePaletteMap _paletteMap;

        public BiomeThemeController(
            IPlayerStateEvents playerEvents,
            ICityNodeResolver cityResolver,
            UiThemeService themeService,
            BiomePaletteMap paletteMap)
        {
            _playerEvents = playerEvents;
            _cityResolver = cityResolver;
            _themeService = themeService;
            _paletteMap = paletteMap;
        }

        public void Initialize()
        {
            _playerEvents.OnCurrentNodeChanged += OnNodeChanged;
        }

        public void Dispose()
        {
            _playerEvents.OnCurrentNodeChanged -= OnNodeChanged;
        }

        private void OnNodeChanged(string nodeId)
        {
            if (!_cityResolver.TryGetCityByNodeId(nodeId, out CityData city))
                return;

            if (_paletteMap.TryGetPalette(city.Biome, out UiColorPalette palette))
                _themeService.SetPalette(palette);
        }
    }
}
