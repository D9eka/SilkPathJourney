using System;
using Internal.Scripts.Player.Languages.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.LanguageSchool
{
    public class LanguageSchoolItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _currentLevelText;
        [SerializeField] private TextMeshProUGUI _availableNextLevelText;
        [SerializeField] private TextMeshProUGUI _daysToLearnText;
        [SerializeField] private Button _learnButton;

        private LanguageType _language;
        private Action<LanguageType> _onLearnClicked;

        public void Initialize(LanguageSchoolEntry entry, Action<LanguageType> callback)
        {
            _language = entry.Language;
            _onLearnClicked = callback;

            if (_nameText != null)
                _nameText.text = entry.Name;

            if (_currentLevelText != null)
                _currentLevelText.text = entry.CurrentLevelFormatted;

            if (_availableNextLevelText != null)
                _availableNextLevelText.text = entry.NextLevelFormatted;

            if (_daysToLearnText != null)
                _daysToLearnText.text = entry.DaysFormatted;

            if (_learnButton != null)
            {
                _learnButton.interactable = entry.CanLearn;
                _learnButton.onClick.AddListener(HandleClick);

                var buttonText = _learnButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                    buttonText.text = entry.LearnButtonText;
            }
        }

        private void HandleClick()
        {
            _onLearnClicked?.Invoke(_language);
        }

        private void OnDestroy()
        {
            if (_learnButton != null)
                _learnButton.onClick.RemoveListener(HandleClick);
        }
    }
}
