using System;
using Internal.Scripts.Npc.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Internal.Scripts.Npc.Trading
{
    public sealed class NpcCaravanAgent : IDisposable
    {
        private readonly RoadAgent _roadAgent;
        private readonly NpcView _view;

        public NpcEconomyState EconomyState { get; }
        public int PrefabIndex { get; }
        public int ColorIndex { get; }

        public string CurrentNodeId => _roadAgent.CurrentNodeId;
        public string DestinationNodeId => _roadAgent.DestinationNodeId;

        public float SpeedMetersPerDay
        {
            get => _roadAgent.SpeedMetersPerDay;
            set => _roadAgent.SpeedMetersPerDay = value;
        }

        public RoadAgent RoadAgent => _roadAgent;

        public event Action<NpcCaravanAgent> OnArrived;

        public NpcCaravanAgent(RoadAgent roadAgent, NpcView view,
            NpcEconomyState economyState, int prefabIndex, int colorIndex)
        {
            _roadAgent = roadAgent;
            _view = view;
            EconomyState = economyState;
            PrefabIndex = prefabIndex;
            ColorIndex = colorIndex;

            _roadAgent.OnArrived += HandleRoadAgentArrived;
        }

        public void SetDestination(string nodeId)
        {
            _roadAgent.SetDestination(nodeId);
        }

        public void DestroyView()
        {
            if (_view != null)
                Object.Destroy(_view.gameObject);
        }

        public void Dispose()
        {
            _roadAgent.OnArrived -= HandleRoadAgentArrived;
        }

        private void HandleRoadAgentArrived(RoadAgent _)
        {
            OnArrived?.Invoke(this);
        }
    }
}
