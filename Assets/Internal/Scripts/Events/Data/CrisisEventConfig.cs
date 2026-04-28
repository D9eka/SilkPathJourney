using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Events.Data
{
    [CreateAssetMenu(menuName = "SPJ/Events/Crisis Event Config", fileName = "CrisisEventConfig")]
    public sealed class CrisisEventConfig : ScriptableObject
    {
        [field: SerializeField] public List<EventData> OnDangerMaxed { get; private set; } = new();
        [field: SerializeField] public List<EventData> OnMoraleDepleted { get; private set; } = new();
        [field: SerializeField] public List<EventData> OnCartBroken { get; private set; } = new();
        [field: SerializeField] public List<EventData> OnFoodDepleted { get; private set; } = new();
        [field: SerializeField] public List<EventData> OnMoneyDepleted { get; private set; } = new();
        [field: SerializeField] public List<EventData> OnCaravanLost { get; private set; } = new();

#if UNITY_EDITOR
        public void ApplyImport(
            List<EventData> onDangerMaxed,
            List<EventData> onMoraleDepleted,
            List<EventData> onCartBroken,
            List<EventData> onFoodDepleted,
            List<EventData> onMoneyDepleted,
            List<EventData> onCaravanLost)
        {
            OnDangerMaxed = onDangerMaxed ?? new List<EventData>();
            OnMoraleDepleted = onMoraleDepleted ?? new List<EventData>();
            OnCartBroken = onCartBroken ?? new List<EventData>();
            OnFoodDepleted = onFoodDepleted ?? new List<EventData>();
            OnMoneyDepleted = onMoneyDepleted ?? new List<EventData>();
            OnCaravanLost = onCaravanLost ?? new List<EventData>();
        }
#endif
    }
}
