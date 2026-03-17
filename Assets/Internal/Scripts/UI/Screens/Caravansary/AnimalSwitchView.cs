using System;
using Internal.Scripts.Caravan.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Caravansary
{
    public class AnimalSwitchView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _effectText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private TextMeshProUGUI _consumptionText;
        [SerializeField] private TextMeshProUGUI _incompatibleBiomesText;
        [SerializeField] private TextMeshProUGUI _neutralBiomesText;
        [SerializeField] private TextMeshProUGUI _compatibleBiomesText;
        [SerializeField] private Button _switchButton;
        [SerializeField] private TextMeshProUGUI _switchButtonText;

        private DraftAnimalType _animalType;
        private Action<DraftAnimalType> _onSwitch;

        public void Initialize(AnimalSwitchData data, Action<DraftAnimalType> onSwitch)
        {
            _animalType = data.AnimalType;
            _onSwitch = onSwitch;

            if (_nameText != null)
                _nameText.text = data.Name;

            if (_effectText != null)
                _effectText.text = data.EffectText;

            if (_priceText != null)
                _priceText.text = data.PriceText;

            if (_consumptionText != null)
                _consumptionText.text = data.ConsumptionText;

            if (_incompatibleBiomesText != null)
                _incompatibleBiomesText.text = data.IncompatibleBiomes;

            if (_neutralBiomesText != null)
                _neutralBiomesText.text = data.NeutralBiomes;

            if (_compatibleBiomesText != null)
                _compatibleBiomesText.text = data.CompatibleBiomes;

            if (_switchButton != null)
            {
                _switchButton.gameObject.SetActive(!data.IsCurrent);
                _switchButton.interactable = data.CanSwitch;
                _switchButton.onClick.AddListener(HandleSwitch);
            }

            if (_switchButtonText != null)
                _switchButtonText.text = data.ButtonText;
        }

        private void HandleSwitch()
        {
            _onSwitch?.Invoke(_animalType);
        }

        private void OnDestroy()
        {
            if (_switchButton != null)
                _switchButton.onClick.RemoveListener(HandleSwitch);
        }
    }
}
