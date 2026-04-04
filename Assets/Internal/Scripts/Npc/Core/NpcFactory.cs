using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Npc.Data;
using Internal.Scripts.Npc.Lifecycle;
using Internal.Scripts.Npc.Movement;
using Internal.Scripts.Npc.Names;
using Internal.Scripts.Npc.NextSegment;
using Internal.Scripts.Player;
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
        private readonly ConvoyPrefabCatalog _catalog;

        public NpcFactory(IRoadPathFinder pathFinder, IRoadNetwork network,
            RoadSamplerCache samplerCache, NpcSimulation simulation, RoadPoseSampler poseSampler,
            IGameDayDeltaProvider gameDayDeltaProvider,
            NpcSimulationSettings settings, WorldCanvas worldCanvas,
            [Zenject.InjectOptional] ConvoyPrefabCatalog catalog)
        {
            _pathFinder = pathFinder;
            _network = network;
            _samplerCache = samplerCache;
            _simulation = simulation;
            _poseSampler = poseSampler;
            _gameDayDeltaProvider = gameDayDeltaProvider;
            _settings = settings;
            _worldCanvas = worldCanvas;
            _catalog = catalog;
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
            if (colorIndex < 0)
                colorIndex = ChooseColorIndex();

            Color color = GetColor(colorIndex);

            int chosenIndex = prefabIndex >= 0 ? prefabIndex : ChoosePrefabIndex();
            NpcView prefab = GetPrefab(chosenIndex);
            NpcView view;
            if (prefab != null)
            {
                view = Object.Instantiate(prefab);
                prefabIndex = chosenIndex;
            }
            else
            {
                var go = new GameObject($"NpcCaravan_{economy.Name}");
                view = go.AddComponent<NpcView>();
                prefabIndex = 0;
            }

            view.ApplyColor(color);
            SpawnCartVisual(view);

            LocalizedString localizedName = !string.IsNullOrEmpty(nameId)
                ? new LocalizedString("Npc", nameId) : null;
            view.InitLabel(_worldCanvas, economy.Name, localizedName);

            RoadAgent roadAgent = Create(view, config, startNodeId);
            return new NpcCaravanAgent(roadAgent, view, economy, prefabIndex, colorIndex);
        }

        private void SpawnCartVisual(NpcView view)
        {
            if (_catalog == null) return;

            var cartClasses = (CartClass[])System.Enum.GetValues(typeof(CartClass));
            if (cartClasses.Length == 0) return;

            CartClass randomCart = cartClasses[Random.Range(0, cartClasses.Length)];
            GameObject cartPrefab = _catalog.GetMainCart(randomCart);
            if (cartPrefab == null) return;

            var meshRenderer = view.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;
            var meshFilter = view.GetComponent<MeshFilter>();
            if (meshFilter != null) meshFilter.mesh = null;

            GameObject cart = Object.Instantiate(cartPrefab, view.VisualRoot);
            cart.transform.localPosition = Vector3.zero;
            cart.transform.localRotation = Quaternion.identity;
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
