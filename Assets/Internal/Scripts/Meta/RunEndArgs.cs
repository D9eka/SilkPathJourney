namespace Internal.Scripts.Meta
{
    public sealed class RunEndArgs
    {
        public readonly EndType EndType;
        public readonly string ReasonKey;
        public readonly RunStatsData Stats;
        public readonly string BranchId;

        public RunEndArgs(EndType endType, string reasonKey, RunStatsData stats, string branchId = null)
        {
            EndType = endType;
            ReasonKey = reasonKey;
            Stats = stats;
            BranchId = branchId;
        }
    }
}
