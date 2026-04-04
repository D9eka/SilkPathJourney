namespace Internal.Scripts.Economy.Cities.Smuggling
{
    public readonly struct SmugglingCheckResult
    {
        public readonly bool WasChecked;
        public readonly bool WasCaught;
        public readonly int ConfiscatedValue;
        public readonly int Penalty;

        private SmugglingCheckResult(bool wasChecked, bool wasCaught, int confiscatedValue, int penalty)
        {
            WasChecked = wasChecked;
            WasCaught = wasCaught;
            ConfiscatedValue = confiscatedValue;
            Penalty = penalty;
        }

        public static SmugglingCheckResult Skipped => new(false, false, 0, 0);
        public static SmugglingCheckResult Passed => new(true, false, 0, 0);
        public static SmugglingCheckResult Caught(int confiscatedValue, int penalty) => new(true, true, confiscatedValue, penalty);
    }
}
