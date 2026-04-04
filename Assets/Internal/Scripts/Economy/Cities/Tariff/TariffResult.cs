namespace Internal.Scripts.Economy.Cities.Tariff
{
    public readonly struct TariffResult
    {
        public readonly bool WasCharged;
        public readonly int Amount;
        public readonly bool InsufficientFunds;

        private TariffResult(bool wasCharged, int amount, bool insufficientFunds)
        {
            WasCharged = wasCharged;
            Amount = amount;
            InsufficientFunds = insufficientFunds;
        }

        public static TariffResult NoTariff => new TariffResult(false, 0, false);
        public static TariffResult Charged(int amount) => new TariffResult(true, amount, false);
        public static TariffResult Insufficient(int required) => new TariffResult(false, required, true);
    }
}
