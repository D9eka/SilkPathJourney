using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Npc.Core;
using Internal.Scripts.Npc.Encounter;
using Internal.Scripts.Road.Core;
using Internal.Scripts.UI.Screens.Trader;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.WorldLabel;
using Plugins.Zenject.Source.Install;
using UnityEngine;

namespace Internal.Scripts.Installers
{
    [CreateAssetMenu(menuName = "SPJ/Installers/Scene Data Installer")]
    public class SceneDataInstaller : ScriptableObjectInstaller<SceneDataInstaller>
    {
        [Header("NPC")]
        [SerializeField] private NpcEncounterSettings _npcEncounterSettings;

        [Header("Player")]
        [SerializeField] private RoadAgentConfig _playerAgentConfig;

        [Header("UI")]
        [SerializeField] private TraderUICatalog _traderUICatalog;

        [Header("World")]
        [SerializeField] private WorldCanvasSettings _worldCanvasSettings;

        [Header("Theme")]
        [SerializeField] private BiomePaletteMap _biomePaletteMap;

        public override void InstallBindings()
        {
            if (_npcEncounterSettings != null)
                Container.BindInstance(_npcEncounterSettings).AsSingle();

            Container.Bind<RoadAgentConfig>().FromInstance(_playerAgentConfig).AsSingle();

            Container.BindInstance(_traderUICatalog).AsSingle();
            Container.BindInstance(_worldCanvasSettings).AsSingle();
            Container.BindInstance(_biomePaletteMap).AsSingle();

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
