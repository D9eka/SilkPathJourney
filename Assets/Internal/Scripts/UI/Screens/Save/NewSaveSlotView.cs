using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Save
{
    public class NewSaveSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private LocalizedString _textLocalizedString;
        [SerializeField] private GameObject _selectionBorder;

        private SaveLoadScreenBase _owner;
        private int _index;
        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _textHandle;

        public void Bind(SaveLoadScreenBase owner, int index, LocalizationService localization)
        {
            _owner = owner;
            _index = index;
            _localization = localization;
        }

        private void OnEnable()
        {
            _textHandle?.Dispose();
            if (_text != null && _localization != null)
                _textHandle = _localization.BindText(_text, _textLocalizedString, "NewSaveSlot.Text");
        }

        private void OnDisable()
        {
            _textHandle?.Dispose();
            _textHandle = null;
        }

        public void SetSelectionState(bool selected)
        {
            if (_selectionBorder != null)
                _selectionBorder.SetActive(selected);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
                return;

            bool isDoubleClick = eventData.clickCount >= 2;
            _owner?.HandleSlotClick(_index, isDoubleClick);
        }
    }
}
