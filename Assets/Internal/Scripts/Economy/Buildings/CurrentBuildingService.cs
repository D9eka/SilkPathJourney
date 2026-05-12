namespace Internal.Scripts.Economy.Buildings
{
    public class CurrentBuildingService
    {
        public BuildingType? Current { get; private set; }
        public void Set(BuildingType b) => Current = b;
        public void Clear() => Current = null;
    }
}
