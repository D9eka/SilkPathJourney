namespace Internal.Scripts.UI.Screens.Shared
{
    public readonly struct CartStatsData
    {
        public readonly string TypeName;
        public readonly string SpeedText;
        public readonly string CapacityText;
        public readonly string DurabilityText;
        public readonly string AnimalsText;
        public readonly string ConsumptionText;

        public CartStatsData(string typeName, string speedText, string capacityText,
            string durabilityText, string animalsText, string consumptionText)
        {
            TypeName = typeName;
            SpeedText = speedText;
            CapacityText = capacityText;
            DurabilityText = durabilityText;
            AnimalsText = animalsText;
            ConsumptionText = consumptionText;
        }
    }
}
