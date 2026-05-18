using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Config;
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
        private readonly CaravanDatabase _caravanDb;
        private readonly GameBalanceConfig _balanceConfig;

        public NpcFactory(IRoadPathFinder pathFinder, IRoadNetwork network,
            RoadSamplerCache samplerCache, NpcSimulation simulation, RoadPoseSampler poseSampler,
            IGameDayDeltaProvider gameDayDeltaProvider,
            NpcSimulationSettings settings, WorldCanvas worldCanvas,
            CaravanDatabase caravanDb, GameBalanceConfig balanceConfig,
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
            _caravanDb = caravanDb;
            _balanceConfig = balanceConfig;
        }

        public RoadAgent Create(NpcView view, RoadAgentConfig config, string startNodeId)
        {
            RoadPathCursor cursor = new RoadPathCursor(_network,
                new SegmentMover(_network, _samplerCache, _poseSampler),
                new NpcNextSegmentProvider(_pathFinder),
                _pathFinder);
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

            CartClass cartClass = ChooseRandomCartClass();
            DraftAnimalType animalType = ChooseRandomAnimalType();
            float computedSpeed = ComputeSpeedFromCartAndAnimal(cartClass, animalType);
            if (computedSpeed > 0f)
                config.SpeedMetersPerDay = computedSpeed;

            SpawnCartVisual(view, cartClass, animalType);

            RoadAgent roadAgent = Create(view, config, startNodeId);
            return new NpcCaravanAgent(roadAgent, view, economy, prefabIndex, colorIndex);
        }

        private CartClass ChooseRandomCartClass()
        {
            if (_caravanDb != null && _caravanDb.CartClasses != null && _caravanDb.CartClasses.Count > 0)
            {
                int idx = Random.Range(0, _caravanDb.CartClasses.Count);
                return _caravanDb.CartClasses[idx].ClassType;
            }

            var values = (CartClass[])System.Enum.GetValues(typeof(CartClass));
            return values.Length > 0 ? values[Random.Range(0, values.Length)] : default;
        }

        private DraftAnimalType ChooseRandomAnimalType()
        {
            var values = (DraftAnimalType[])System.Enum.GetValues(typeof(DraftAnimalType));
            DraftAnimalType picked;
            int guard = 0;
            do
            {
                picked = values[Random.Range(0, values.Length)];
                guard++;
            } while (picked == DraftAnimalType.Unknown && guard < 8);
            return picked;
        }

        private float ComputeSpeedFromCartAndAnimal(CartClass cartClass, DraftAnimalType animalType)
        {
            if (_caravanDb == null || _balanceConfig == null) return 0f;

            CartClassData cartData = _caravanDb.CartClasses?.Find(c => c.ClassType == cartClass);
            if (cartData == null) return 0f;

            float speedKmDay = cartData.SpeedKmDay;
            DraftAnimalData animal = _caravanDb.GetDraftAnimal(animalType);
            if (animal != null)
                speedKmDay *= 1f + animal.SpeedModPct / 100f;

            float jitter = Random.Range(0.9f, 1.1f);
            return speedKmDay * _balanceConfig.WorldUnitsPerKm * jitter;
        }

        private void SpawnCartVisual(NpcView view, CartClass cartClass, DraftAnimalType animalType)
        {
            if (_catalog == null) return;

            GameObject cartPrefab = _catalog.GetMainCart(cartClass);
            if (cartPrefab == null) return;

            var meshRenderer = view.GetComponent<MeshRenderer>();
            if (meshRenderer != null) meshRenderer.enabled = false;
            var meshFilter = view.GetComponent<MeshFilter>();
            if (meshFilter != null) meshFilter.mesh = null;

            GameObject cart = Object.Instantiate(cartPrefab, view.VisualRoot);
            cart.transform.localPosition = Vector3.zero;
            cart.transform.localRotation = Quaternion.identity;

            var cartView = cart.GetComponent<CartFollowerView>();
            if (cartView != null)
            {
                var entry = _catalog.GetAnimal(animalType);
                if (entry.Prefab != null)
                    cartView.SetAnimal(entry.Prefab, entry.Scale > 0 ? entry.Scale : 1f, entry.Offset);
            }
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
