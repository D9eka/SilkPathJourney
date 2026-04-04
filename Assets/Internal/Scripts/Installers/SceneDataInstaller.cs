using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.Smuggling;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Encounter;
using Internal.Scripts.Road.Core;
using Internal.Scripts.UI.Screens.Trader;
using Internal.Scripts.Player;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.WorldLabel;
using Plugins.Zenject.Source.Install;
using UnityEngine;

namespace Internal.Scripts.Installers
{
    [CreateAssetMenu(menuName = "SPJ/Installers/Scene Data Installer")]
    public class SceneDataInstaller : ScriptableObjectInstaller<SceneDataInstaller>
    {
        [Header("Economy")]
        [SerializeField] private SmugglingDetectionSettings _smuDetectionSettings;
        [SerializeField] private SmugglingPenaltySettings _smuPenaltySettings;

        [Header("NPC")]
        [SerializeField] private NpcEncounterSettings _npcEncounterSettings;

        [Header("Player")]
        [SerializeField] private RoadAgentConfig _playerAgentConfig;

        [Header("UI")]
        [SerializeField] private TraderUICatalog _traderUICatalog;

        [Header("World")]
        [SerializeField] private WorldCanvasSettings _worldCanvasSettings;

        [Header("Convoy")]
        [SerializeField] private ConvoyPrefabCatalog _convoyPrefabCatalog;

        [Header("Theme")]
        [SerializeField] private BiomePaletteMap _biomePaletteMap;

        public override void InstallBindings()
        {
            Container.BindInstance(_smuDetectionSettings).AsSingle();
            Container.BindInstance(_smuPenaltySettings).AsSingle();
            Container.BindInstance(_npcEncounterSettings).AsSingle();

            Container.Bind<RoadAgentConfig>().FromInstance(_playerAgentConfig).AsSingle();

            Container.BindInstance(_traderUICatalog).AsSingle();
            Container.BindInstance(_worldCanvasSettings).AsSingle();
            Container.BindInstance(_biomePaletteMap).AsSingle();

            if (_convoyPrefabCatalog != null)
                Container.BindInstance(_convoyPrefabCatalog).AsSingle();

            InstallTheme();
        }

        private void InstallTheme()
        {
            Container.Bind<StaticColorController>().AsSingle();

            _biomePaletteMap.TryGetPalette(Biome.Plains, out var defaultPalette);
            Container.Bind<UiThemeService>().AsSingle()
                .WithArguments(defaultPalette).NonLazy();

            Container.BindInterfacesTo<BiomeThemeController>().AsSingle().NonLazy();
        }
    }
}
