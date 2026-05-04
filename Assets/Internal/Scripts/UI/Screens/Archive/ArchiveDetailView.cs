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
            gameObject.SetActive(true);
            if (_emptyStatePlaceholder != null) _emptyStatePlaceholder.SetActive(false);
            if (_titleText != null) _titleText.text = title;
            if (_descriptionText != null) _descriptionText.text = description;
        }

        public void ShowEmpty(string message)
        {
            gameObject.SetActive(false);
            if (_emptyStatePlaceholder != null) _emptyStatePlaceholder.SetActive(true);
            if (_emptyStateText != null) _emptyStateText.text = message;
        }
    }
}
