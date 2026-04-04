using UnityEngine;

namespace Internal.Scripts.Economy.Cities.Smuggling
{
    [CreateAssetMenu(menuName = "SPJ/Economy/Smuggling Penalty Settings")]
    public sealed class SmugglingPenaltySettings : ScriptableObject
    {
        [SerializeField] private int _minPenaltyAmount = 50;
        [SerializeField] private float _confiscationFraction = 0.2f;
        [SerializeField] private int _reputationPenalty = 10;
        [SerializeField] private int _banDurationDays = 5;

        public int MinPenaltyAmount => _minPenaltyAmount;
        public float ConfiscationFraction => _confiscationFraction;
        public int ReputationPenalty => _reputationPenalty;
        public int BanDurationDays => _banDurationDays;
    }
}
