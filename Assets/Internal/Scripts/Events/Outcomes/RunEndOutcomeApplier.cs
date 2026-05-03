using System;
using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Meta;
using Internal.Scripts.Meta.Achievements;
using Internal.Scripts.Quests;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Screens.Core.Config;
using Internal.Scripts.UI.StackService;
using Internal.Scripts.World.State;

namespace Internal.Scripts.Events.Outcomes
{
    public sealed class RunEndOutcomeApplier : IOutcomeApplier
    {
        private readonly GameClock _gameClock;
        private readonly RunStatsService _runStatsService;
        private readonly SaveRepository _saveRepository;
        private readonly ScreenStackService _screenStackService;
        private readonly PersistentProgressService _persistent;
        private readonly LegacyPointsCalculator _legacyCalculator;
        private readonly AchievementService _achievementService;
        private readonly QuestPendingEndingsService _pendingEndings;
        private bool _ended;
        private EndType? _pendingEndType;
        private string _pendingBranchId;

        public bool IsEnded => _ended;

        public IEnumerable<EventOutcomeType> SupportedTypes => new[] { EventOutcomeType.EndRun };

        public RunEndOutcomeApplier(
            GameClock gameClock,
            RunStatsService runStatsService,
            SaveRepository saveRepository,
            ScreenStackService screenStackService,
            PersistentProgressService persistent,
            LegacyPointsCalculator legacyCalculator,
            AchievementService achievementService,
            QuestPendingEndingsService pendingEndings)
        {
            _gameClock = gameClock;
            _runStatsService = runStatsService;
            _saveRepository = saveRepository;
            _screenStackService = screenStackService;
            _persistent = persistent;
            _legacyCalculator = legacyCalculator;
            _achievementService = achievementService;
            _pendingEndings = pendingEndings;
        }

        public void Apply(EventOutcomeEntry entry)
        {
            string paramStr = entry.Param ?? string.Empty;
            int sep = paramStr.IndexOf('|');
            string endTypeStr = sep < 0 ? paramStr : paramStr.Substring(0, sep);
            string branchId = sep < 0 ? null : paramStr.Substring(sep + 1);

            if (!Enum.TryParse<EndType>(endTypeStr, out var endType) || endType == EndType.None)
                return;
            _pendingEndType = endType;
            _pendingBranchId = branchId;
        }

        public void TryFlushPending()
        {
            if (_pendingEndType == null) return;
            EndType endType = _pendingEndType.Value;
            string branchId = _pendingBranchId;
            _pendingEndType = null;
            _pendingBranchId = null;
            TriggerRunEnd(endType, branchId);
        }

        public void TriggerRunEnd(EndType endType, string branchId = null)
        {
            if (_ended) return;
            _ended = true;
            _pendingEndings.ClearAllPendingEndings();

            string reasonKey = endType switch
            {
                EndType.Victory => "UI.RunEnd.Reason.Victory",
                EndType.Defeat_Bankruptcy => "UI.RunEnd.Reason.Bankruptcy",
                EndType.Defeat_Mutiny => "UI.RunEnd.Reason.Mutiny",
                EndType.Defeat_CaravanLost => "UI.RunEnd.Reason.CaravanLost",
                EndType.Defeat_Famine => "UI.RunEnd.Reason.Famine",
                EndType.Defeat_CaravanDisbanded => "UI.RunEnd.Reason.CaravanDisbanded",
                _ => "UI.RunEnd.Reason.Bankruptcy"
            };

            _gameClock.Pause();
            _runStatsService.Snapshot();
            RunStatsData stats = _runStatsService.Stats;
            stats.EndType = endType;
            stats.EndReasonKey = reasonKey;
            stats.EndingBranchId = branchId;

            int earned = _legacyCalculator.Calculate(stats, endType, out int bonus);
            stats.LegacyEarned = earned;
            stats.LegacyBonus = bonus;
            _persistent.AddLegacyPoints(earned);
            _persistent.RecordRunCompleted(stats, endType, earned);
            _achievementService.CheckAll(stats, endType);
            _saveRepository.Save();

            var args = new RunEndArgs(endType, reasonKey, stats, branchId);
            _screenStackService.TryOpen(ScreenId.RunEnd, args, out _);
        }
    }
}
