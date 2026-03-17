using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Tavern
{
    public class RumorView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private LocalizedString _headerLocalizedString;
        [SerializeField] private TextMeshProUGUI _text;

        public void Initialize(string rumorsText)
        {
            if (_headerText != null)
                _headerText.text = _headerLocalizedString != null
                    ? LocalizationService.ResolveString(_headerLocalizedString, "UI.Tavern.Rumors.Header.Main", "Tavern.RumorsHeader")
                    : "UI.Tavern.Rumors.Header.Main";

            if (_text != null)
                _text.text = rumorsText;

            gameObject.SetActive(!string.IsNullOrEmpty(rumorsText));
        }
    }
}
