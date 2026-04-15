using System.Collections.Generic;
using System.Text;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.Quests.Generated;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;

namespace Internal.Scripts.UI.Screens.Quests
{
    public struct QuestsViewState
    {
        public List<QuestViewData> Quests;
        public string SelectedQuestId;

        public static QuestsViewState Empty => new() { Quests = new List<QuestViewData>() };
    }

    public struct QuestViewData
    {
        public string QuestId;
        public string BranchName;
        public int OrderInBranch;
        public int TotalStages;
        public int CurrentStageIndex;
        public bool IsSelected;
        public bool IsTracked;
        public bool IsFailed;
        public bool IsCompleted;
    }

    public static class QuestLocContext
    {
        public static string QuestName(string questId) => $"Quest.{questId}.Name";
        public static string StageDesc(string questId, string stageId) => $"Quest.{questId}.Stage.{stageId}";
        public static string QuestDesc(string questId) => $"Quest.{questId}.Description";
        public static string BranchName(QuestBranch branch) => $"QuestBranch.{branch}.Name";
        public static string BranchDesc(QuestBranch branch) => $"QuestBranch.{branch}.Description";
    }

    public static class QuestRewardFormatter
    {
        public static string FormatReward(QuestRewardData reward, string bullet = "")
        {
            string prefix = string.IsNullOrEmpty(bullet) ? "" : bullet + " ";
            string text;
            switch (reward.Type)
            {
                case QuestRewardType.Money:
                    text = $"{reward.Value} {LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Resource_Money)}";
                    break;
                case QuestRewardType.Reputation:
                    text = $"+{reward.Value} {LocalizationService.Resolve(LocUI.Table, LocUI.UI_Global_Resource_Reputation)}";
                    break;
                case QuestRewardType.Item:
                    text = $"{reward.Target} ×{reward.Value}";
                    break;
                case QuestRewardType.Status:
                    text = reward.Target;
                    break;
                case QuestRewardType.UnlockRoute:
                    text = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Quest_Reward_Route).Replace("{target}", reward.Target);
                    break;
                case QuestRewardType.UnlockItem:
                    text = LocalizationService.Resolve(LocUI.Table, LocUI.UI_Quest_Reward_UnlockItem).Replace("{target}", reward.Target);
                    break;
                default:
                    text = reward.Value != 0
                        ? $"{reward.Target} ×{reward.Value}"
                        : reward.Target;
                    break;
            }
            return prefix + text;
        }

        public static string FormatRewards(IReadOnlyList<QuestRewardData> rewards, string separator = ", ", string bullet = "")
        {
            if (rewards == null || rewards.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            for (int i = 0; i < rewards.Count; i++)
            {
                if (i > 0) sb.Append(separator);
                sb.Append(FormatReward(rewards[i], bullet));
            }
            return sb.ToString();
        }
    }
}
