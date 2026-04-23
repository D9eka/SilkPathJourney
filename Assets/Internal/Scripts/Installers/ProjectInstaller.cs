using Internal.Scripts.Camp;
using Internal.Scripts.Camera;
using Internal.Scripts.Camera.Zoom;
using Internal.Scripts.Caravan;
using Internal.Scripts.Config;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.Economy.Simulation;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Npc.Names;
using Internal.Scripts.Player;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Save;
using Internal.Scripts.Travel.Hazards;
using Internal.Scripts.Travel.Pickups;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Config;
using Internal.Scripts.Utils;
using Plugins.Zenject.Source.Install;
using UnityEngine;

namespace Internal.Scripts.Installers
{
    [CreateAssetMenu(menuName = "SPJ/Installers/Project Installer")]
    public class ProjectInstaller : ScriptableObjectInstaller<ProjectInstaller>
    {
        [Header("Camera")]
        [SerializeField] private CameraZoomerData _cameraZoomerData;
        [SerializeField] private CameraSceneSettings _cameraSceneSettings;

        [Header("Economy")]
        [SerializeField] private EconomyDatabase _economyDatabase;
        [SerializeField] private EconomySimulationSettings _economySimulationSettings;
        [SerializeField] private CultureAdjacencyData _cultureAdjacencyData;

        [Header("Events")]
        [SerializeField] private EventDatabase _eventDatabase;
        [SerializeField] private CrisisEventConfig _crisisEventConfig;

        [Header("Quests")]
        [SerializeField] private QuestDatabase _questDatabase;

        [Header("Caravan")]
        [SerializeField] private CaravanDatabase _caravanDatabase;

        [Header("Player")]
        [SerializeField] private PlayerConfig _playerProfile;

        [Header("NPC")]
        [SerializeField] private NpcSimulationSettings _npcSimulationSettings;
        [SerializeField] private NameDatabase _nameDatabase;

        [Header("Camp")]
        [SerializeField] private CampActionDatabase _campActionDatabase;

        [Header("Guild")]
        [SerializeField] private GuildSettings _guildSettings;

        [Header("UI")]
        [SerializeField] private ScreenCatalog _screenCatalog;
        [SerializeField] private ResourceIconCatalog _resourceIconCatalog;

        [Header("Scenes")]
        [SerializeField] private SceneReference _gameScene;
        [SerializeField] private SceneReference _mainMenuScene;

        [Header("Balance")]
        [SerializeField] private GameBalanceConfig _gameBalanceConfig;
        [SerializeField] private TimeSpeedConfig _timeSpeedConfig;
        [SerializeField] private CaravanSpeedConfig _caravanSpeedConfig;

        [Header("Pickups")]
        [SerializeField] private PickupDatabase _pickupDatabase;

        [Header("Hazards")]
        [SerializeField] private HazardDatabase _hazardDatabase;

        public override void InstallBindings()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);

            Container.BindInstance(_cameraZoomerData).AsSingle();
            Container.BindInstance(_cameraSceneSettings).AsSingle();
            Container.BindInstance(_economyDatabase).AsSingle();
            Container.BindInstance(_economySimulationSettings).AsSingle();
            Container.BindInstance(_cultureAdjacencyData).AsSingle();
            Container.BindInstance(_eventDatabase).AsSingle();
            Container.BindInstance(_crisisEventConfig).AsSingle();
            Container.BindInstance(_questDatabase).AsSingle();
            Container.BindInstance(_caravanDatabase).AsSingle();
            Container.BindInstance(_playerProfile).AsSingle();
            Container.BindInstance(_npcSimulationSettings).AsSingle();
            Container.BindInstance(_nameDatabase).AsSingle();
            Container.BindInstance(_campActionDatabase).AsSingle();
            Container.BindInstance(_guildSettings).AsSingle();
            Container.BindInstance(_screenCatalog).AsSingle();
            Container.BindInstance(_resourceIconCatalog).AsSingle();
            Container.BindInstance(_gameBalanceConfig).AsSingle();
            Container.BindInstance(_timeSpeedConfig).AsSingle();
            Container.BindInstance(_caravanSpeedConfig).AsSingle();
            Container.BindInstance(_pickupDatabase).AsSingle();
            Container.BindInstance(_hazardDatabase).AsSingle();

            Container.Bind<ISaveService>().To<JsonSaveService>().AsSingle();
            Container.Bind<ActiveSaveSlot>().AsSingle();
            Container.Bind<SaveRepository>().AsSingle();
            Container.Bind<LocalizationService>().AsSingle();
            Container.Bind<QuitGameService>().AsSingle();
            Container.Bind<SceneReference>().WithId(SceneRefId.Game).FromInstance(_gameScene).AsCached();
            Container.Bind<SceneReference>().WithId(SceneRefId.MainMenu).FromInstance(_mainMenuScene).AsCached();
        }
    }
}
