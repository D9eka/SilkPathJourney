namespace Internal.Scripts.UI.StackService
{
    public enum ScreenOpenResult
    {
        Success = 0,
        BlockedByExclusive = 1,
        AlreadyOpen = 2,
        MissingConfig = 3,
        MissingPrefab = 4,
        MissingView = 5,
        MissingViewModel = 6,
        MissingRoot = 7
    }
}
