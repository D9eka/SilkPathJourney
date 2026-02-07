using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Events.Data
{
    [CreateAssetMenu(menuName = "SPJ/Events/Event Data", fileName = "EventData")]
    public class EventData : ScriptableObject
    {
        [Header("Event Info")]
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; }
        [field: SerializeField] public LocalizedString EventType { get; private set; }
        [field: SerializeField] public LocalizedString Description { get; private set; }
        [field: SerializeField] public Sprite Image { get; private set; }

        [Header("Choices")]
        [field: SerializeField] public List<EventChoice> Choices { get; private set; }

        [Header("Conditions")]
        [field: SerializeField] public List<EventCondition> Conditions { get; private set; }
        [field: SerializeField] public float Weight { get; private set; } = 1f;

#if UNITY_EDITOR
        public void ApplyImport(
            string id,
            LocalizedString name,
            LocalizedString eventType,
            LocalizedString description,
            Sprite image,
            List<EventChoice> choices,
            List<EventCondition> conditions,
            float weight)
        {
            Id = id;
            Name = name;
            EventType = eventType;
            Description = description;
            Image = image;
            Choices = choices ?? new List<EventChoice>();
            Conditions = conditions ?? new List<EventCondition>();
            Weight = weight;
        }
#endif
    }

    [Serializable]
    public struct EventChoice
    {
        [field: SerializeField] public LocalizedString Text { get; private set; }
        [field: SerializeField] public List<EventCondition> Conditions { get; private set; }
        [field: SerializeField] public List<EventOutcomeEntry> Outcomes { get; private set; }

#if UNITY_EDITOR
        public EventChoice(LocalizedString text, List<EventCondition> conditions, List<EventOutcomeEntry> outcomes)
        {
            Text = text;
            Conditions = conditions ?? new List<EventCondition>();
            Outcomes = outcomes ?? new List<EventOutcomeEntry>();
        }
#endif
    }

    [Serializable]
    public struct EventCondition
    {
        [field: SerializeField] public EventConditionType Type { get; private set; }
        [field: SerializeField] public string Param { get; private set; }
        [field: SerializeField] public float Value { get; private set; }

#if UNITY_EDITOR
        public EventCondition(EventConditionType type, string param, float value)
        {
            Type = type;
            Param = param;
            Value = value;
        }
#endif
    }

    [Serializable]
    public struct EventOutcomeEntry
    {
        [field: SerializeField] public EventOutcomeType Type { get; private set; }
        [field: SerializeField] public string Param { get; private set; }
        [field: SerializeField] public float Value { get; private set; }

#if UNITY_EDITOR
        public EventOutcomeEntry(EventOutcomeType type, string param, float value)
        {
            Type = type;
            Param = param;
            Value = value;
        }
#endif
    }

}
