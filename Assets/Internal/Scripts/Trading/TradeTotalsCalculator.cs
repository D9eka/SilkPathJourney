using System.Collections.Generic;
using Internal.Scripts.Items;

namespace Internal.Scripts.Trading
{
    public sealed class TradeTotalsCalculator
    {
        private readonly ItemCatalog _itemCatalog;

        public TradeTotalsCalculator(ItemCatalog itemCatalog)
        {
            _itemCatalog = itemCatalog;
        }

        public int CalculateTotal(IReadOnlyDictionary<string, int> counts)
        {
            int total = 0;
            if (counts == null) return total;

            foreach (KeyValuePair<string, int> kvp in counts)
            {
                if (kvp.Value <= 0) continue;

                total += _itemCatalog.GetItemPrice(kvp.Key) * kvp.Value;
            }

            return total;
        }

        public bool HasPlayerFunds(int buyTotal, int sellTotal, int playerMoney)
        {
            int net = playerMoney + sellTotal - buyTotal;
            return net >= 0;
        }

        public bool HasNpcFunds(int buyTotal, int sellTotal, int npcMoney)
        {
            int net = npcMoney - sellTotal + buyTotal;
            return net >= 0;
        }
    }
}
