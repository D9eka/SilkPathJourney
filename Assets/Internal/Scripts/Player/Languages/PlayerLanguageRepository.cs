using System;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.Save;
using R3;
using Zenject;

namespace Internal.Scripts.Player.Languages
{
    public sealed class PlayerLanguageRepository : IInitializable, IDisposable
    {
        private readonly SaveRepository _saveRepository;
        private readonly ReactiveProperty<PlayerLanguageState> _state = new(new PlayerLanguageState());
        private bool _isLoaded;

        public PlayerLanguageRepository(SaveRepository saveRepository)
        {
            _saveRepository = saveRepository;
        }

        public void Initialize() => EnsureLoaded();

        public Observable<PlayerLanguageState> StateStream => _state;
        public PlayerLanguageState Current => _state.Value;

        public void SetLanguage(LanguageType type, LanguageProficiency proficiency)
        {
            EnsureLoaded();
            _state.Value.SetProficiency(type, proficiency);
            _saveRepository.Save();
            _state.ForceNotify();
        }

        private void EnsureLoaded()
        {
            if (_isLoaded)
                return;

            SaveData root = _saveRepository.Data;
            root.Languages ??= new PlayerLanguageState();
            _state.Value = root.Languages;
            _isLoaded = true;
        }

        public void Dispose() => _state?.Dispose();
    }
}
