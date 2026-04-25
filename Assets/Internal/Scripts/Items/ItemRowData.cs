using Internal.Scripts.Economy.Generated;

namespace Internal.Scripts.Items
{
    public readonly struct ItemRowData
    {
        public readonly string ItemId;
        public readonly int Count;
        public readonly string Name;
        public readonly string Weight;
        public readonly string Price;
        public readonly string PriceTooltipTitle;
        public readonly string PriceTooltipText;
        public readonly ItemType Category;

        public ItemRowData(string itemId, int count, string name, string weight, string price,
            string priceTooltipTitle = null, string priceTooltipText = null,
            ItemType category = ItemType.Unknown)
        {
            ItemId = itemId;
            Count = count;
            Name = name;
            Weight = weight;
            Price = price;
            PriceTooltipTitle = priceTooltipTitle;
            PriceTooltipText = priceTooltipText;
            Category = category;
        }
    }
}
