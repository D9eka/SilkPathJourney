using Internal.Scripts.Caravan;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.NewGame
{
    public class CartClassButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private GameObject _selectedBorder;

        private CartClassData _data;
        private NewGameScreenViewModel _vm;

        public void SetData(CartClassData data, NewGameScreenViewModel vm, bool isSelected)
        {
            _data = data;
            _vm = vm;

            _nameText.text = LocalizationService.ResolveString(data.Name, data.Id, $"CartClass.{data.Id}.Name");
            _descriptionText.text = LocalizationService.ResolveString(data.Description, data.Id, $"CartClass.{data.Id}.Description");
            _selectedBorder.SetActive(isSelected);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
        }

        public void SetSelected(bool selected)
        {
            _selectedBorder.SetActive(selected);
        }

        private void OnClick() => _vm.SelectedCartClass.Value = _data;
    }
}
