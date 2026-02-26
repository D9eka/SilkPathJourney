using System;

namespace Internal.Scripts.Save
{
    public sealed class ActiveSaveSlot
    {
        public string SlotId { get; private set; }

        public void Set(string slotId)
        {
            SlotId = slotId;
        }

        public string CreateNew()
        {
            SlotId = Guid.NewGuid().ToString("N");
            return SlotId;
        }
    }
}
