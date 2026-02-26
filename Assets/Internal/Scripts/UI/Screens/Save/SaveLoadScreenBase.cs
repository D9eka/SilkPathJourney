using System.Collections.Generic;
using Internal.Scripts.Save;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Save
{
    public abstract class SaveLoadScreenBase : PopupScreen
    {
        [Header("Slots")]
        [SerializeField] private Transform _slotsContainer;
        [SerializeField] private SaveSlotView _slotPrefab;

        [Header("Action")]
        [SerializeField] private Button _actionButton;
        [SerializeField] private TextMeshProUGUI _actionButtonText;

        protected SaveLoadScreenViewModel ViewModel { get; private set; }
        protected Transform SlotsContainer => _slotsContainer;

        private readonly List<SaveSlotView> _spawnedSlots = new();
        private int _selectedIndex = -1;
        private UnityAction _actionHandler;
        private bool _subscribed;

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            ViewModel = viewModel as SaveLoadScreenViewModel;
            SubscribeViewModel();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            base.OnDisable();
        }

        private void SubscribeViewModel()
        {
            if (ViewModel == null || _subscribed)
                return;

            _subscribed = true;
            ViewModel.SlotsChanged += RefreshSlots;

            _actionHandler = OnActionButtonPressed;
            _actionButton.onClick.AddListener(_actionHandler);

            RefreshSlots();
        }

        private void UnsubscribeViewModel()
        {
            if (ViewModel == null)
                return;

            _subscribed = false;
            ViewModel.SlotsChanged -= RefreshSlots;

            if (_actionHandler != null)
            {
                _actionButton.onClick.RemoveListener(_actionHandler);
                _actionHandler = null;
            }
        }

        protected void RefreshSlots()
        {
            List<SaveMetadata> saves = ViewModel.GetSaves();
            int count = saves.Count;

            EnsurePool(count);

            for (int i = 0; i < count; i++)
            {
                SaveSlotView view = _spawnedSlots[i];
                view.gameObject.SetActive(true);
                view.Bind(this, i);

                string cityName = ViewModel.ResolveCityName(saves[i].CurrentNodeId);
                view.SetData(saves[i], cityName, ViewModel.ResourceIcons);
            }

            for (int i = count; i < _spawnedSlots.Count; i++)
                _spawnedSlots[i].gameObject.SetActive(false);

            OnRefreshSlots(count);

            if (_actionButtonText != null)
                _actionButtonText.text = GetActionButtonText();

            if (_selectedIndex >= TotalSlotCount())
                _selectedIndex = TotalSlotCount() - 1;

            UpdateSelection();
        }

        protected virtual void OnRefreshSlots(int activeSaveCount)
        {
        }

        protected abstract string GetActionButtonText();

        protected abstract void PerformAction(int index);

        protected int ActiveSaveSlotCount()
        {
            int count = 0;
            for (int i = 0; i < _spawnedSlots.Count; i++)
            {
                if (_spawnedSlots[i].gameObject.activeSelf)
                    count++;
            }
            return count;
        }

        protected virtual int TotalSlotCount()
        {
            return ActiveSaveSlotCount();
        }

        protected SaveSlotView GetSlot(int index)
        {
            if (index >= 0 && index < _spawnedSlots.Count && _spawnedSlots[index].gameObject.activeSelf)
                return _spawnedSlots[index];
            return null;
        }

        internal void HandleSlotClick(int index, bool isDoubleClick)
        {
            SelectIndex(index);

            if (isDoubleClick)
                PerformAction(index);
        }

        internal void HandleSlotDelete(int index)
        {
            if (index < 0 || index >= _spawnedSlots.Count)
                return;

            SaveSlotView slot = _spawnedSlots[index];
            if (!slot.gameObject.activeSelf || slot.Metadata == null)
                return;

            ViewModel.DeleteSlot(slot.Metadata.SlotId);
        }

        private void SelectIndex(int index)
        {
            int total = TotalSlotCount();
            if (total == 0)
            {
                _selectedIndex = -1;
                UpdateSelection();
                return;
            }

            _selectedIndex = Mathf.Clamp(index, 0, total - 1);
            UpdateSelection();
        }

        private void UpdateSelection()
        {
            for (int i = 0; i < _spawnedSlots.Count; i++)
            {
                bool selected = i == _selectedIndex && _spawnedSlots[i].gameObject.activeSelf;
                _spawnedSlots[i].SetSelectionState(selected);
            }

            OnUpdateSelection(_selectedIndex, ActiveSaveSlotCount());
            _actionButton.interactable = _selectedIndex >= 0;
        }

        protected virtual void OnUpdateSelection(int selectedIndex, int activeSaveCount)
        {
        }

        private void OnActionButtonPressed()
        {
            if (_selectedIndex >= 0)
                PerformAction(_selectedIndex);
        }

        private void EnsurePool(int count)
        {
            while (_spawnedSlots.Count < count)
            {
                SaveSlotView view = Instantiate(_slotPrefab, _slotsContainer);
                SetupColors(view.gameObject);
                _spawnedSlots.Add(view);
            }
        }

        protected void SetupColors(GameObject go)
        {
            if (ViewModel == null)
                return;

            foreach (UiColorBinder binder in go.GetComponentsInChildren<UiColorBinder>(true))
                binder.Initialize(ViewModel.ThemeService);

            foreach (UiStaticColorBinder binder in go.GetComponentsInChildren<UiStaticColorBinder>(true))
                binder.Initialize(ViewModel.ColorController);
        }
    }
}
