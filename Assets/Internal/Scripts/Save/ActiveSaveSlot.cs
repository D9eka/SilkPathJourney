namespace Internal.Scripts.Save
{
    public sealed class ActiveSaveSlot
    {
        public string SlotId { get; private set; }
        public string SelectedBackgroundId { get; private set; }
        public string SelectedCartClassId { get; private set; }

        public void Set(string slotId)
        {
            SlotId = slotId;
        }

        public string CreateNew(string backgroundId = null, string cartClassId = null)
        {
            SlotId = JsonSaveService.RunSlotId;
            SelectedBackgroundId = backgroundId;
            SelectedCartClassId = cartClassId;
            return SlotId;
        }

        public void Restore(string backgroundId, string cartClassId)
        {
            SelectedBackgroundId = backgroundId;
            SelectedCartClassId = cartClassId;
        }
    }
}
