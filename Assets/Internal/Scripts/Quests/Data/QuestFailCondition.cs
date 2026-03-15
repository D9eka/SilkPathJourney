using System;
using Internal.Scripts.Quests.Generated;
using UnityEngine;

namespace Internal.Scripts.Quests.Data
{
    [Serializable]
    public struct QuestFailCondition
    {
        [field: SerializeField] public QuestFailConditionType Type { get; private set; }
        [field: SerializeField] public string Param { get; private set; }
        [field: SerializeField] public int Value { get; private set; }

#if UNITY_EDITOR
        public QuestFailCondition(QuestFailConditionType type, string param, int value)
        {
            Type = type;
            Param = param;
            Value = value;
        }
#endif
    }
}
