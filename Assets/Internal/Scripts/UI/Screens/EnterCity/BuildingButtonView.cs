using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.EnterCity
{
    public class BuildingButtonView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _button;

        private BuildingEntry _entry;
        private Action<BuildingEntry> _onClicked;

        public void Initialize(BuildingEntry entry, Action<BuildingEntry> onClicked)
        {
            _entry = entry;
            _onClicked = onClicked;

            if (_nameText != null)
                _nameText.text = entry.Name;

            if (_descriptionText != null)
                _descriptionText.text = entry.Description;

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            _onClicked?.Invoke(_entry);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}
