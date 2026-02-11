using DG.Tweening;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.Hud
{
    public class MinorEventView : MonoBehaviour, IEventToastView
    {
        [SerializeField] private TextMeshProUGUI _eventText;
        [SerializeField] private Image _eventIcon;
        [SerializeField] private GameObject _root;
        [SerializeField] private float _displayDuration = 3f;

        private Sequence _sequence;
        private GameObject Root => _root != null ? _root : gameObject;

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
                _eventText.text = LocalizationHelper.ResolveString(
                    eventData.Name, eventData.Id, "EventToast");

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
