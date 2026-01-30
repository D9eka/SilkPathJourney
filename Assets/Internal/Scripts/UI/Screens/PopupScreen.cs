using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens
{
    public abstract class PopupScreen : MonoBehaviour
    {
        [Header("Headers")]
        [SerializeField] private GameObject _mainHeader;
        [SerializeField] private GameObject _additionalHeader;
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _mainHeaderText;
        [SerializeField] private TextMeshProUGUI _additionalHeaderText;
        [Header("Buttons")]
        [SerializeField] private Button _closeButton;
        [Header("Content")]
        [SerializeField] private OverlayScreen _overlayScreen;
        [Header("LocalizedStrings")]
        [SerializeField] private LocalizedString _mainHeaderLocalizedString;
        [SerializeField] private LocalizedString _additionalHeaderLocalizedString;

        public void SetAdditionalHeaderState(bool state)
        {
            _additionalHeader.SetActive(state);
        }
    }
}