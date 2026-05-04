using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Journal
{
    public class JournalDayItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _dayLabelText;
        [SerializeField] private Transform _linesContent;
        [SerializeField] private IconLabel _lineItemPrefab;

        [Header("Localization")]
        [SerializeField] private LocalizedString _dayLabelLocalizedString;

        private JournalIconCatalog _catalog;

        public void Bind(JournalIconCatalog catalog)
        {
            _catalog = catalog;
        }

        public void Initialize(JournalDayEntry entry)
        {
            string label = LocalizationService.Resolve("UI", "ui.journal.day_label", $"День {entry.Day}", entry.Day);
            _dayLabelText.text = label;

            if (_linesContent == null || _lineItemPrefab == null)
                return;

            foreach (var line in entry.Lines)
            {
                var instance = Instantiate(_lineItemPrefab, _linesContent);
                Sprite icon = _catalog?.Get(line.Type);
                instance.Initialize(icon, line.Text);
            }
        }
    }
}
