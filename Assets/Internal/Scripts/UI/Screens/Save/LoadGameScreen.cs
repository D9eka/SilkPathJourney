using Internal.Scripts.Save;

namespace Internal.Scripts.UI.Screens.Save
{
    public class LoadGameScreen : SaveLoadScreenBase
    {
        protected override void PerformAction(int index)
        {
            SaveSlotView slot = GetSlot(index);
            if (slot?.Metadata != null)
                ViewModel.PerformAction(slot.Metadata.SlotId);
        }
    }
}
