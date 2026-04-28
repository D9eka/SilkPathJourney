namespace Internal.Scripts.Meta
{
    public sealed class RunEndArgs
    {
        public readonly EndType EndType;
        public readonly string ReasonKey;
        public readonly RunStatsData Stats;

        public RunEndArgs(EndType endType, string reasonKey, RunStatsData stats)
        {
            EndType = endType;
            ReasonKey = reasonKey;
            Stats = stats;
        }
    }
}
