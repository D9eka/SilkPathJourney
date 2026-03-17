using Internal.Scripts.Caravan;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.Economy;

namespace Internal.Scripts.Player
{
    public sealed class DraftAnimalService
    {
        private readonly PlayerResourceRepository _resourceRepo;
        private readonly CaravanDatabase _caravanDb;

        public DraftAnimalService(PlayerResourceRepository resourceRepo, CaravanDatabase caravanDb)
        {
            _resourceRepo = resourceRepo;
            _caravanDb = caravanDb;
        }

        public bool SwitchAnimal(DraftAnimalType newType)
        {
            DraftAnimalData animalData = _caravanDb.GetDraftAnimal(newType);
            if (animalData == null)
                return false;

            var state = _resourceRepo.Current;
            if (state.Money < animalData.Price)
                return false;

            _resourceRepo.UpdateResources(s =>
            {
                s.Money -= animalData.Price;
                s.DraftAnimalId = animalData.Id;
            });

            return true;
        }

        public float GetSpeedModifier()
        {
            DraftAnimalData data = GetCurrentAnimal();
            return data != null ? 1f + data.SpeedModPct / 100f : 1f;
        }

        public float GetCapacityModifier()
        {
            DraftAnimalData data = GetCurrentAnimal();
            return data != null ? 1f + data.CapacityModPct / 100f : 1f;
        }

        public float GetFeedPerDay()
        {
            DraftAnimalData data = GetCurrentAnimal();
            return data?.FeedPerDay ?? 0f;
        }

        private DraftAnimalData GetCurrentAnimal()
        {
            return _caravanDb.GetDraftAnimalById(_resourceRepo.Current.DraftAnimalId);
        }
    }
}
