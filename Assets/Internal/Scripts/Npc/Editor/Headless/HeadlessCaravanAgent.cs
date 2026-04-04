using System;
using Internal.Scripts.Npc.Data;

namespace Internal.Scripts.Npc.Editor.Headless
{
    public sealed class HeadlessCaravanAgent
    {
        public NpcEconomyState EconomyState { get; }
        public string CurrentNodeId { get; private set; }
        public string DestinationNodeId { get; private set; }
        public float SpeedMetersPerDay { get; set; }
        public float RemainingTravelDays { get; private set; }
        public int PrefabIndex { get; }
        public int ColorIndex { get; }

        public event Action<HeadlessCaravanAgent> OnArrived;

        public HeadlessCaravanAgent(NpcEconomyState economy, string startNodeId, float speed, int prefabIndex, int colorIndex)
        {
            EconomyState = economy;
            CurrentNodeId = startNodeId;
            SpeedMetersPerDay = speed;
            PrefabIndex = prefabIndex;
            ColorIndex = colorIndex;
        }

        public void SetDestination(string nodeId, RoadGraphSnapshot snapshot)
        {
            DestinationNodeId = nodeId;
            float distance = snapshot.GetDistance(CurrentNodeId, nodeId);
            RemainingTravelDays = distance >= float.MaxValue || SpeedMetersPerDay <= 0f
                ? float.MaxValue
                : distance / SpeedMetersPerDay;
        }

        public void AdvanceDay()
        {
            if (string.IsNullOrEmpty(DestinationNodeId)) return;

            RemainingTravelDays -= 1f;

            if (RemainingTravelDays <= 0f)
            {
                CurrentNodeId = DestinationNodeId;
                DestinationNodeId = null;
                OnArrived?.Invoke(this);
            }
        }
    }
}
