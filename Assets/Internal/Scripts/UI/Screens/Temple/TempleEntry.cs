namespace Internal.Scripts.UI.Screens.Temple
{
    public enum TempleServiceKind { Pray, Donate, Bless }

    public readonly struct TempleEntry
    {
        public readonly TempleServiceKind Kind;
        public readonly string Name;
        public readonly string Description;
        public readonly string ButtonText;
        public readonly bool CanAfford;

        public TempleEntry(TempleServiceKind kind, string name, string description, string buttonText, bool canAfford)
        {
            Kind = kind;
            Name = name;
            Description = description;
            ButtonText = buttonText;
            CanAfford = canAfford;
        }
    }
}
