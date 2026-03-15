using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Quests.Generated;
using UnityEngine;

namespace Internal.Scripts.Quests.Data
{
    [CreateAssetMenu(menuName = "SPJ/Quests/Quest Database", fileName = "QuestDatabase")]
    public class QuestDatabase : ScriptableObject
    {
        [field: SerializeField] public List<QuestData> Quests { get; private set; }

        private Dictionary<string, QuestData> _byId;

        public QuestData GetById(string id)
        {
            if (Quests == null || Quests.Count == 0) return null;
            _byId ??= Quests.ToDictionary(q => q.Id);
            return _byId.TryGetValue(id, out var quest) ? quest : null;
        }

        public List<QuestData> GetByBranch(QuestBranch branch)
        {
            return Quests.Where(q => q.Branch == branch).ToList();
        }

#if UNITY_EDITOR
        public void ApplyImport(List<QuestData> quests)
        {
            Quests = quests ?? new List<QuestData>();
            _byId = null;
        }
#endif
    }
}
