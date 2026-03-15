using System;
using Internal.Scripts.Quests.Generated;
using UnityEngine;

namespace Internal.Scripts.Quests.Data
{
    [Serializable]
    public struct QuestStageCondition
    {
        [field: SerializeField] public QuestStageConditionType Type { get; private set; }
        [field: SerializeField] public string Param { get; private set; }
        [field: SerializeField] public int Value { get; private set; }

#if UNITY_EDITOR
        public QuestStageCondition(QuestStageConditionType type, string param, int value)
        {
            Type = type;
            Param = param;
            Value = value;
        }
#endif
    }
}
