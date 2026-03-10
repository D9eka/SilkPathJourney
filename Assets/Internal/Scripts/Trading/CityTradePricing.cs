using Internal.Scripts.Economy.Simulation;

namespace Internal.Scripts.Trading
{
    public sealed class CityTradePricing : ITradePricing
    {
        private readonly CityTradePriceService _service;
        private readonly string _cityId;

        public CityTradePricing(CityTradePriceService service, string cityId)
        {
            _service = service;
            _cityId = cityId;
        }

        public int GetBuyPrice(string itemId)
            => _service.GetPrice(_cityId, itemId, TradePriceKind.BuyFromCity);

        public int GetSellPrice(string itemId)
            => _service.GetPrice(_cityId, itemId, TradePriceKind.SellToCity);

        public PriceBreakdown GetBuyBreakdown(string itemId)
            => _service.GetPriceBreakdown(_cityId, itemId, TradePriceKind.BuyFromCity);

        public PriceBreakdown GetSellBreakdown(string itemId)
            => _service.GetPriceBreakdown(_cityId, itemId, TradePriceKind.SellToCity);
    }
}
