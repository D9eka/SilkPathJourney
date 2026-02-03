namespace Internal.Scripts.Items
{
    public struct ItemRowData
    {
        public string ItemId;
        public int Count;
        public string Name;
        public string Weight;
        public string Price;

        public ItemRowData(string itemId, int count, string name, string weight, string price)
        {
            ItemId = itemId;
            Count = count;
            Name = name;
            Weight = weight;
            Price = price;
        }
    }
}
