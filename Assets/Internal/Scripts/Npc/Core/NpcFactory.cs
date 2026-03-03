using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Npc.Movement;
using Internal.Scripts.Npc.Names;
using Internal.Scripts.Npc.NextSegment;
using Internal.Scripts.Npc.Trading;
using Internal.Scripts.Road.Graph;
using Internal.Scripts.Road.Path;
using Internal.Scripts.UI.WorldLabel;
using Internal.Scripts.World.State;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Npc.Core
{
    public sealed class NpcFactory
    {
        private readonly IRoadPathFinder _pathFinder;
        private readonly IRoadNetwork _network;
        private readonly RoadSamplerCache _samplerCache;
        private readonly NpcSimulation _simulation;
        private readonly RoadPoseSampler _poseSampler;
        private readonly IGameDayDeltaProvider _gameDayDeltaProvider;
        private readonly NpcSimulationSettings _settings;
        private readonly WorldCanvas _worldCanvas;

        public NpcFactory(IRoadPathFinder pathFinder, IRoadNetwork network,
            RoadSamplerCache samplerCache, NpcSimulation simulation, RoadPoseSampler poseSampler,
            IGameDayDeltaProvider gameDayDeltaProvider,
            NpcSimulationSettings settings, WorldCanvas worldCanvas)
        {
            _pathFinder = pathFinder;
            _network = network;
            _samplerCache = samplerCache;
            _simulation = simulation;
            _poseSampler = poseSampler;
            _gameDayDeltaProvider = gameDayDeltaProvider;
            _settings = settings;
            _worldCanvas = worldCanvas;
        }

        public RoadAgent Create(NpcView view, RoadAgentConfig config, string startNodeId)
        {
            RoadPathCursor cursor = new RoadPathCursor(_network,
                new SegmentMover(_network, _samplerCache, _poseSampler),
                new NpcNextSegmentProvider(_pathFinder));
            RoadAgent agent = new RoadAgent(view, config, cursor, _gameDayDeltaProvider, startNodeId);
            _simulation.Register(agent);
            return agent;
        }

        public RoadAgent CreateFromPrefab(NpcView prefab, RoadAgentConfig config, string startNodeId, Color color)
        {
            NpcView instance = Object.Instantiate(prefab);
            instance.ApplyColor(color);
            return Create(instance, config, startNodeId);
        }

        public NpcCaravanAgent CreateCaravan(
            RoadAgentConfig config, string startNodeId,
            NpcEconomyState economy, string nameId,
            int prefabIndex = -1, int colorIndex = -1)
        {
            if (prefabIndex < 0)
                prefabIndex = ChoosePrefabIndex();

            NpcView prefab = GetPrefab(prefabIndex);
            if (prefab == null)
                return null;

            if (colorIndex < 0)
                colorIndex = ChooseColorIndex();

            Color color = GetColor(colorIndex);
            NpcView view = Object.Instantiate(prefab);
            view.ApplyColor(color);

            LocalizedString localizedName = !string.IsNullOrEmpty(nameId)
                ? new LocalizedString("Npc", nameId) : null;
            view.InitLabel(_worldCanvas, economy.Name, localizedName);

            RoadAgent roadAgent = Create(view, config, startNodeId);
            return new NpcCaravanAgent(roadAgent, view, economy, prefabIndex, colorIndex);
        }

        private int ChoosePrefabIndex()
        {
            if (_settings.Prefabs == null || _settings.Prefabs.Count == 0)
                return -1;
            return Random.Range(0, _settings.Prefabs.Count);
        }

        private NpcView GetPrefab(int index)
        {
            if (_settings.Prefabs == null || _settings.Prefabs.Count == 0)
                return null;
            index = Mathf.Clamp(index, 0, _settings.Prefabs.Count - 1);
            return _settings.Prefabs[index];
        }

        private int ChooseColorIndex()
        {
            if (_settings.AvailableColors == null || _settings.AvailableColors.Count == 0)
                return 0;
            return Random.Range(0, _settings.AvailableColors.Count);
        }

        private Color GetColor(int index)
        {
            if (_settings.AvailableColors == null || _settings.AvailableColors.Count == 0)
                return Color.white;
            index = Mathf.Clamp(index, 0, _settings.AvailableColors.Count - 1);
            return _settings.AvailableColors[index];
        }
    }
}
