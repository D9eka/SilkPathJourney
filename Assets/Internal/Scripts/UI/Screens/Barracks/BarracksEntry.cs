namespace Internal.Scripts.UI.Screens.Barracks
{
    public enum BarracksServiceKind { HireGuard }

    public readonly struct BarracksEntry
    {
        public readonly BarracksServiceKind Kind;
        public readonly string Name;
        public readonly string Description;
        public readonly string ButtonText;
        public readonly bool CanAfford;

        public BarracksEntry(BarracksServiceKind kind, string name, string description, string buttonText, bool canAfford)
        {
            Kind = kind;
            Name = name;
            Description = description;
            ButtonText = buttonText;
            CanAfford = canAfford;
        }
    }

    public readonly struct EliteGuardEntry
    {
        public readonly string Name;
        public readonly string Description;
        public readonly string ButtonText;
        public readonly bool CanAfford;
        public readonly bool IsOwned;

        public EliteGuardEntry(string name, string description, string buttonText, bool canAfford, bool isOwned)
        {
            Name = name;
            Description = description;
            ButtonText = buttonText;
            CanAfford = canAfford;
            IsOwned = isOwned;
        }
    }
}
