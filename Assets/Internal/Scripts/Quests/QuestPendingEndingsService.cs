using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Internal.Scripts.Economy.Buildings;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Quests.Generated;
using Internal.Scripts.Save;

namespace Internal.Scripts.Quests
{
    public class QuestPendingEndingsService
    {
        private readonly SaveRepository _saveRepository;
        private readonly QuestDatabase _questDatabase;

        private const int BranchGiverOrder = 1;
        private const string FinaleOfferEventFormat = "event_{0}_finale_offer";

        public static string GetFinaleOfferEventId(string branchId) =>
            string.Format(FinaleOfferEventFormat, branchId);

        public event Action PendingEndingsChanged;

        public QuestPendingEndingsService(SaveRepository saveRepository, QuestDatabase questDatabase)
        {
            _saveRepository = saveRepository;
            _questDatabase = questDatabase;
        }

        public void AddPendingEnding(string branchId)
        {
            if (string.IsNullOrEmpty(branchId)) return;
            if (_saveRepository.Data.Quests.PendingEndingBranchIds.Contains(branchId)) return;
            _saveRepository.Data.Quests.PendingEndingBranchIds.Add(branchId);
            _saveRepository.Save();
            PendingEndingsChanged?.Invoke();
        }

        public void ClearAllPendingEndings()
        {
            if (_saveRepository.Data.Quests.PendingEndingBranchIds.Count == 0) return;
            _saveRepository.Data.Quests.PendingEndingBranchIds.Clear();
            _saveRepository.Save();
            PendingEndingsChanged?.Invoke();
        }

        public IEnumerable<string> GetPendingEndingsForBuilding(BuildingType building, string cityId)
        {
            var pendingIds = _saveRepository.Data.Quests.PendingEndingBranchIds;
            if (_questDatabase?.Quests == null || pendingIds.Count == 0)
                yield break;

            foreach (var branchId in pendingIds)
            {
                var branch = ParseBranch(branchId);
                if (branch == QuestBranch.None) continue;

                foreach (var quest in _questDatabase.Quests)
                {
                    if (quest.Branch != branch) continue;
                    if (quest.OrderInBranch != BranchGiverOrder) continue;
                    if (quest.GiverBuilding != building) continue;
                    if (quest.StartCityId != cityId) continue;
                    yield return branchId;
                    break;
                }
            }
        }

        private static QuestBranch ParseBranch(string branchId)
        {
            if (string.IsNullOrEmpty(branchId)) return QuestBranch.None;
            string pascal = string.Concat(branchId.Split('_')
                .Select(w => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(w)));
            if (!Enum.TryParse<QuestBranch>(pascal, out var result))
            {
                UnityEngine.Debug.LogWarning($"[SPJ Quests] Unknown branch id: '{branchId}' (parsed as '{pascal}')");
                return QuestBranch.None;
            }
            return result;
        }
    }
}
