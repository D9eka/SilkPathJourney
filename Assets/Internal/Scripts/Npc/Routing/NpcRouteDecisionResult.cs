namespace Internal.Scripts.Npc.Routing
{
    public enum NpcRouteFallbackKind
    {
        None,
        NearestCity,
        EmergencyCity
    }

    public readonly struct NpcRouteDecisionResult
    {
        public NpcRouteDecisionResult(string targetNodeId, NpcRouteFallbackKind fallbackKind)
        {
            TargetNodeId = targetNodeId;
            FallbackKind = fallbackKind;
        }

        public string TargetNodeId { get; }
        public NpcRouteFallbackKind FallbackKind { get; }
        public bool HasTarget => !string.IsNullOrEmpty(TargetNodeId);
    }
}
