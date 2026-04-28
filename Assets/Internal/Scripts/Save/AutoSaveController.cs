using Internal.Scripts.Economy.Cities;
using Zenject;

namespace Internal.Scripts.Save
{
    public sealed class AutoSaveController : IInitializable, System.IDisposable
    {
        private readonly SaveRepository _saveRepository;
        private readonly ICityEntryService _cityEntryService;

        public AutoSaveController(SaveRepository saveRepository, ICityEntryService cityEntryService)
        {
            _saveRepository = saveRepository;
            _cityEntryService = cityEntryService;
        }

        public void Initialize()
        {
            _cityEntryService.OnCityEntered += SaveOnCityEntry;
        }

        public void Dispose()
        {
            _cityEntryService.OnCityEntered -= SaveOnCityEntry;
        }

        private void SaveOnCityEntry(CityData _) => _saveRepository.SaveRun();
    }
}
