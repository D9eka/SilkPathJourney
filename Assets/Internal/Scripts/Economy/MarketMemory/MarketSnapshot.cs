using System;
using System.Collections.Generic;

namespace Internal.Scripts.Economy.MarketMemory
{
    [Serializable]
    public class MarketSnapshot
    {
        public string CityId;
        public int Day;
        public List<ItemPriceRecord> Prices = new();
        public bool IsRumor;
    }

    [Serializable]
    public class ItemPriceRecord
    {
        public string ItemId;
        public int BuyPrice;
        public int SellPrice;
        public bool IsAvailable;
    }
}
