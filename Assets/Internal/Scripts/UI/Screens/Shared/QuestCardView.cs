using System;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Shared
{
    public class QuestCardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private LocalizedString _headerLocalizedString;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Button _talkButton;
        [SerializeField] private TextMeshProUGUI _talkButtonText;
        [SerializeField] private LocalizedString _talkLocalizedString;

        private Action _onTalk;
        private bool _listenerAdded;

        public void Initialize(string description, Action onTalk)
        {
            _onTalk = onTalk;
            gameObject.SetActive(true);

            if (_headerText != null)
                _headerText.text = _headerLocalizedString != null
                    ? LocalizationService.ResolveString(_headerLocalizedString, "UI.Tavern.Quest.Header.Main", "QuestCard.Header")
                    : "UI.Tavern.Quest.Header.Main";

            if (_descriptionText != null)
                _descriptionText.text = description;

            if (_talkButtonText != null)
                _talkButtonText.text = _talkLocalizedString != null
                    ? LocalizationService.ResolveString(_talkLocalizedString, "UI.Tavern.Quest.Button.Talk", "QuestCard.Talk")
                    : "UI.Tavern.Quest.Button.Talk";

            if (_talkButton != null && !_listenerAdded)
            {
                _talkButton.onClick.AddListener(HandleTalk);
                _listenerAdded = true;
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HandleTalk()
        {
            _onTalk?.Invoke();
        }

        private void OnDestroy()
        {
            if (_talkButton != null)
                _talkButton.onClick.RemoveListener(HandleTalk);
        }
    }
}
