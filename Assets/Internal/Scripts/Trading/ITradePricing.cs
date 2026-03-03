namespace Internal.Scripts.Trading
{
    public interface ITradePricing
    {
        int GetBuyPrice(string itemId);
        int GetSellPrice(string itemId);
    }
}
