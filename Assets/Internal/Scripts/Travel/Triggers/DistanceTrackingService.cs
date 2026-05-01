namespace Internal.Scripts.Travel.Triggers
{
    public sealed class DistanceTrackingService
    {
        public float TotalDistanceUnits { get; private set; }
        public void Add(float units) => TotalDistanceUnits += units;
    }
}
