using System;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Archive
{
    public class ArchiveListItemView : MonoBehaviour
    {
        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Header("Selection")]
        [SerializeField] private GameObject _border;
        [SerializeField] private Button _cardButton;
        [SerializeField] private Button _selectButton;
        [SerializeField] private TextMeshProUGUI _selectButtonText;

        private string _id;
        private Action<string> _onSelected;

        public void Initialize(ArchiveListEntry entry, Action<string> onSelected)
        {
            _id = entry.Id;
            _onSelected = onSelected;

            _nameText.text = entry.DisplayName;

            bool hasDesc = !string.IsNullOrEmpty(entry.Description);
            _descriptionText.gameObject.SetActive(hasDesc);
            if (hasDesc)
                _descriptionText.text = entry.Description;

            if (_border != null) _border.SetActive(false);

            if (_selectButtonText != null)
                _selectButtonText.text = LocalizationService.Resolve("UI", "ui.archive.select", "Выбрать");

            _selectButton.onClick.AddListener(HandleClick);
            if (_cardButton != null) _cardButton.onClick.AddListener(HandleClick);
        }

        public void SetSelected(bool isSelected)
        {
            _selectButton.interactable = !isSelected;
            if (_cardButton != null) _cardButton.interactable = !isSelected;
            if (_border != null) _border.SetActive(isSelected);
        }

        private void HandleClick()
        {
            _onSelected?.Invoke(_id);
        }

        private void OnDestroy()
        {
            _selectButton.onClick.RemoveListener(HandleClick);
            if (_cardButton != null) _cardButton.onClick.RemoveListener(HandleClick);
        }
    }
}
