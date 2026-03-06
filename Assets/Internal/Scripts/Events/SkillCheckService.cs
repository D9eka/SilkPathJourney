using Internal.Scripts.Config;
using Internal.Scripts.Player.Skills;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Internal.Scripts.Events
{
    public class SkillCheckService
    {
        private readonly PlayerSkillRepository _skillRepository;
        private readonly GameBalanceConfig _balanceConfig;

        public SkillCheckService(PlayerSkillRepository skillRepository, GameBalanceConfig balanceConfig)
        {
            _skillRepository = skillRepository;
            _balanceConfig = balanceConfig;
        }

        public float CalculateSkillChance(SkillType skillType, float baseChance)
        {
            if (skillType == SkillType.None) return 1f;
            int skill = _skillRepository.Current.GetSkill(skillType);
            float bonus = skill * _balanceConfig.SkillBonusPerPoint;
            return Mathf.Min(baseChance + bonus, _balanceConfig.SkillChanceCap);
        }

        public bool RollSkillCheck(SkillType skillType, float baseChance)
        {
            return Random.value <= CalculateSkillChance(skillType, baseChance);
        }
    }
}
