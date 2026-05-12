using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Trader
{
    public class ProfileItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _contentText;

        private LocalizationService.LocalizedTextHandle _headerHandle;

        public void Initialize(LocalizationService localization, LocalizedString header, string content)
        {
            _headerHandle?.Dispose();
            _headerHandle = null;

            if (_headerText != null && header != null && localization != null)
                _headerHandle = localization.BindText(_headerText, header, "ProfileItem.Header");

            if (_contentText != null)
                _contentText.text = content ?? "\u2014";
        }

        private void OnDestroy()
        {
            _headerHandle?.Dispose();
        }
    }
}
