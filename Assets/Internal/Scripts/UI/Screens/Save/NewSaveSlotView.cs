using UnityEngine;
using UnityEngine.EventSystems;

namespace Internal.Scripts.UI.Screens.Save
{
    public class NewSaveSlotView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameObject _selectionBorder;

        private SaveLoadScreenBase _owner;
        private int _index;

        public void Bind(SaveLoadScreenBase owner, int index)
        {
            _owner = owner;
            _index = index;
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
