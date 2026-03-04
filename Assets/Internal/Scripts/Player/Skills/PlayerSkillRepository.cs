using System;
using Internal.Scripts.Config;
using Internal.Scripts.Save;
using R3;
using Zenject;

namespace Internal.Scripts.Player.Skills
{
    public sealed class PlayerSkillRepository : IInitializable, IDisposable
    {
        private readonly SaveRepository _saveRepository;
        private readonly GameBalanceConfig _config;
        private readonly ReactiveProperty<PlayerSkillState> _state = new(new PlayerSkillState());
        private bool _isLoaded;

        public PlayerSkillRepository(SaveRepository saveRepository, GameBalanceConfig config)
        {
            _saveRepository = saveRepository;
            _config = config;
        }

        public void Initialize() => EnsureLoaded();

        public Observable<PlayerSkillState> StateStream => _state;
        public PlayerSkillState Current => _state.Value;

        public void AddSkill(SkillType type, int amount)
        {
            UpdateSkills(s => s.AddSkill(type, amount, _config.MaxSkill));
        }

        public void UpdateSkills(Action<PlayerSkillState> mutator)
        {
            if (mutator == null)
                return;

            EnsureLoaded();
            mutator(_state.Value);
            _saveRepository.Save();
            _state.ForceNotify();
        }

        private void EnsureLoaded()
        {
            if (_isLoaded)
                return;

            SaveData root = _saveRepository.Data;
            root.Skills ??= new PlayerSkillState();
            _state.Value = root.Skills;
            _isLoaded = true;
        }

        public void Dispose() => _state?.Dispose();
    }
}
