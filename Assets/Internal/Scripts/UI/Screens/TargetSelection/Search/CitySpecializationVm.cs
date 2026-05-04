using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Generated;

namespace Internal.Scripts.UI.Screens.TargetSelection.Search
{
    public sealed class CitySpecializationVm
    {
        private const int MaxPerGroup = 3;

        public IReadOnlyList<ItemType> CheapToBuy { get; }
        public IReadOnlyList<ItemType> ExpensiveToBuy { get; }
        public IReadOnlyList<ItemType> CheapToSell { get; }
        public IReadOnlyList<ItemType> ExpensiveToSell { get; }

        public bool HasPurchaseRow => CheapToBuy.Count > 0 || ExpensiveToBuy.Count > 0;
        public bool HasSalesRow => CheapToSell.Count > 0 || ExpensiveToSell.Count > 0;

        private CitySpecializationVm(
            IReadOnlyList<ItemType> cheapToBuy,
            IReadOnlyList<ItemType> expensiveToBuy,
            IReadOnlyList<ItemType> cheapToSell,
            IReadOnlyList<ItemType> expensiveToSell)
        {
            CheapToBuy = cheapToBuy;
            ExpensiveToBuy = expensiveToBuy;
            CheapToSell = cheapToSell;
            ExpensiveToSell = expensiveToSell;
        }

        public static CitySpecializationVm Build(CityTypeData typeData)
        {
            if (typeData == null || typeData.CategoryCoefs == null || typeData.CategoryCoefs.Count == 0)
                return Empty();

            var filtered = new List<CityTypeData.CategoryCoef>(typeData.CategoryCoefs.Count);
            foreach (var c in typeData.CategoryCoefs)
            {
                if (c.Category == ItemType.Unknown || c.Category == ItemType.Quest)
                    continue;
                filtered.Add(c);
            }

            var byBuyAsc = new List<CityTypeData.CategoryCoef>(filtered);
            var byBuyDesc = new List<CityTypeData.CategoryCoef>(filtered);
            var bySellAsc = new List<CityTypeData.CategoryCoef>(filtered);
            var bySellDesc = new List<CityTypeData.CategoryCoef>(filtered);

            byBuyAsc.Sort((a, b) => a.BuyCoef.CompareTo(b.BuyCoef));
            byBuyDesc.Sort((a, b) => b.BuyCoef.CompareTo(a.BuyCoef));
            bySellAsc.Sort((a, b) => a.SellCoef.CompareTo(b.SellCoef));
            bySellDesc.Sort((a, b) => b.SellCoef.CompareTo(a.SellCoef));

            return new CitySpecializationVm(
                ToCategories(byBuyAsc, MaxPerGroup),
                ToCategories(byBuyDesc, MaxPerGroup),
                ToCategories(bySellAsc, MaxPerGroup),
                ToCategories(bySellDesc, MaxPerGroup));
        }

        private static IReadOnlyList<ItemType> ToCategories(List<CityTypeData.CategoryCoef> src, int max)
        {
            int count = Math.Min(src.Count, max);
            if (count == 0) return Array.Empty<ItemType>();
            var result = new ItemType[count];
            for (int i = 0; i < count; i++)
                result[i] = src[i].Category;
            return result;
        }

        private static CitySpecializationVm Empty() =>
            new(Array.Empty<ItemType>(), Array.Empty<ItemType>(),
                Array.Empty<ItemType>(), Array.Empty<ItemType>());
    }
}
