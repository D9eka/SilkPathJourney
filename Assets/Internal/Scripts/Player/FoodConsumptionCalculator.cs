using Internal.Scripts.Caravan;
using Internal.Scripts.Economy.Save;

namespace Internal.Scripts.Player
{
    public sealed class FoodConsumptionCalculator
    {
        private readonly CaravanDatabase _caravanDatabase;

        public FoodConsumptionCalculator(CaravanDatabase caravanDatabase)
        {
            _caravanDatabase = caravanDatabase;
        }

        public float Calculate(PlayerResourceState state)
        {
            float total = 1f;
            total += CalculateCaravanAnimalFeed(state);
            total += state.Companions?.Count ?? 0;
            return total;
        }

        private float CalculateCaravanAnimalFeed(PlayerResourceState state)
        {
            DraftAnimalData animal = _caravanDatabase.GetDraftAnimalById(state.DraftAnimalId);
            if (animal == null)
                return 0f;

            float feedPerDay = animal.FeedPerDay;

            CartClassData classData = _caravanDatabase.GetCartClassById(state.CartClassId);
            float total = (classData?.AnimalCount ?? 0) * feedPerDay;

            if (state.Carts != null)
            {
                foreach (CartState cart in state.Carts)
                    total += cart.AnimalCount * feedPerDay;
            }

            return total;
        }
    }
}
