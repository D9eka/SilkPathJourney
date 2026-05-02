using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Archive
{
    public class ArchiveListItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
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

            _selectButton.onClick.AddListener(HandleClick);
        }

        public void SetSelected(bool isSelected)
        {
            _selectButton.interactable = !isSelected;
        }

        private void HandleClick()
        {
            _onSelected?.Invoke(_id);
        }

        private void OnDestroy()
        {
            _selectButton.onClick.RemoveListener(HandleClick);
        }
    }
}
