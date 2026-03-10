using DG.Tweening;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Hud
{
    public class MinorEventView : MonoBehaviour, IEventToastView
    {
        [SerializeField] private TextMeshProUGUI _eventText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Image _eventIcon;
        [SerializeField] private GameObject _root;
        [SerializeField] private float _displayDuration = 5f;

        private EventOutcomeFormatter _outcomeFormatter;

        private Sequence _sequence;
        private GameObject Root => _root != null ? _root : gameObject;

        public void SetOutcomeFormatter(EventOutcomeFormatter formatter)
        {
            _outcomeFormatter = formatter;
        }

        private void Awake()
        {
            Root.SetActive(false);
        }

        public void Show(EventData eventData)
        {
            KillSequence();

            Root.SetActive(true);

            if (_eventIcon != null)
                _eventIcon.sprite = eventData.Image;

            if (_eventText != null)
                _eventText.text = LocalizationService.ResolveString(
                    eventData.Name, eventData.Id, "EventToast");

            if (_descriptionText != null)
            {
                string description = LocalizationService.ResolveString(
                    eventData.Description, eventData.Id, "EventToastDesc");

                string summary = eventData.AutoOutcomes.Count > 0
                    ? _outcomeFormatter.BuildOutcomeSummary(eventData.AutoOutcomes)
                    : null;

                _descriptionText.text = summary != null
                    ? $"{description}. {summary}"
                    : description;
            }

            _sequence = DOTween.Sequence()
                .AppendInterval(_displayDuration)
                .OnComplete(Hide)
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        private void Hide()
        {
            Root.SetActive(false);
        }

        private void KillSequence()
        {
            if (_sequence != null && _sequence.IsActive())
                _sequence.Kill();
            _sequence = null;
        }

        private void OnDestroy() => KillSequence();
    }
}
