using Internal.Scripts.Config;

namespace Internal.Scripts.Player.Skills
{
    public sealed class TradePriceSkillModifier
    {
        private readonly PlayerSkillRepository _skillRepository;
        private readonly GameBalanceConfig _config;

        public TradePriceSkillModifier(PlayerSkillRepository skillRepository, GameBalanceConfig config)
        {
            _skillRepository = skillRepository;
            _config = config;
        }

        public float GetBuyMultiplier()
        {
            int trade = _skillRepository.Current.GetSkill(SkillType.Trade);
            return 1f - trade / _config.TradePriceDivisor;
        }

        public float GetSellMultiplier()
        {
            int trade = _skillRepository.Current.GetSkill(SkillType.Trade);
            return 1f + trade / _config.TradePriceDivisor;
        }
    }
}
