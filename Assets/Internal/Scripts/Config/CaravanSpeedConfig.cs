using System;
using Internal.Scripts.Player;
using UnityEngine;

namespace Internal.Scripts.Config
{
    [Serializable]
    public struct SpeedModeData
    {
        public float SpeedMultiplier;
        public float WearMultiplier;
        public float FoodMultiplier;
        public float DangerPerDay;
    }

    [CreateAssetMenu(menuName = "SPJ/Caravan Speed Config", fileName = "CaravanSpeedConfig")]
    public sealed class CaravanSpeedConfig : ScriptableObject
    {
        [Header("Mode: Camp")]
        [SerializeField] private SpeedModeData _camp = new()
            { SpeedMultiplier = 0f, WearMultiplier = 0f, FoodMultiplier = 1f, DangerPerDay = 0f };

        [Header("Mode: Normal")]
        [SerializeField] private SpeedModeData _normal = new()
            { SpeedMultiplier = 1f, WearMultiplier = 1f, FoodMultiplier = 1f, DangerPerDay = 0f };

        [Header("Mode: Rush")]
        [SerializeField] private SpeedModeData _rush = new()
            { SpeedMultiplier = 1.5f, WearMultiplier = 2f, FoodMultiplier = 2f, DangerPerDay = 2f };

        [Header("Durability")]
        [field: SerializeField] public float BaseDurabilityWearRate { get; private set; } = 0.02f;

        [Header("Overload")]
        [field: SerializeField] public float MaxOverloadRatio { get; private set; } = 1.5f;
        [field: SerializeField] public float OverloadSpeedPenalty { get; private set; } = 0.5f;
        [field: SerializeField] public float OverloadWearBonus { get; private set; } = 0.25f;
        [field: SerializeField] public float OverloadFoodBonus { get; private set; } = 0.25f;

        public SpeedModeData GetModeData(CaravanSpeedMode mode) => mode switch
        {
            CaravanSpeedMode.Camp => _camp,
            CaravanSpeedMode.Normal => _normal,
            CaravanSpeedMode.Rush => _rush,
            _ => _normal
        };
    }
}
