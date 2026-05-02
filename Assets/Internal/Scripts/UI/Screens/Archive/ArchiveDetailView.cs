using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Archive
{
    public class ArchiveDetailView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private GameObject _emptyStatePlaceholder;
        [SerializeField] private TextMeshProUGUI _emptyStateText;

        public void ShowEntry(string title, string description)
        {
            _emptyStatePlaceholder.SetActive(false);
            _titleText.text = title;
            _descriptionText.text = description;
        }

        public void ShowEmpty(string message)
        {
            _emptyStatePlaceholder.SetActive(true);
            _emptyStateText.text = message;
            _titleText.text = string.Empty;
            _descriptionText.text = string.Empty;
        }
    }
}
