using Internal.Scripts.Save;
using Internal.Scripts.UI.Theme;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Save
{
    public class SaveGameScreen : SaveLoadScreenBase
    {
        [Header("New Slot")]
        [SerializeField] private NewSaveSlotView _newSlotPrefab;

        private NewSaveSlotView _spawnedNewSlot;

        protected override int TotalSlotCount()
        {
            return ActiveSaveSlotCount() + 1;
        }

        protected override void OnRefreshSlots(int activeSaveCount)
        {
            if (_spawnedNewSlot == null)
            {
                _spawnedNewSlot = Instantiate(_newSlotPrefab, SlotsContainer);
                _spawnedNewSlot.gameObject.InitializeColorBinders(ViewModel?.ThemeService, ViewModel?.ColorController);
            }

            _spawnedNewSlot.gameObject.SetActive(true);
            _spawnedNewSlot.Bind(this, activeSaveCount, Localization);
            _spawnedNewSlot.transform.SetAsFirstSibling();
        }

        protected override void OnUpdateSelection(int selectedIndex, int activeSaveCount)
        {
            if (_spawnedNewSlot != null && _spawnedNewSlot.gameObject.activeSelf)
                _spawnedNewSlot.SetSelectionState(selectedIndex == activeSaveCount);
        }

        protected override void PerformAction(int index)
        {
            int activeSaveCount = ActiveSaveSlotCount();

            if (index == activeSaveCount)
            {
                ViewModel.SaveToNewSlot();
            }
            else
            {
                SaveSlotView slot = GetSlot(index);
                if (slot?.Metadata != null)
                    ViewModel.PerformAction(slot.Metadata.SlotId);
            }
        }
    }
}
