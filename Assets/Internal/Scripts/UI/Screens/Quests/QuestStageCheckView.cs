using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Quests
{
    public enum StageDisplayState
    {
        Completed,
        Current,
        Future
    }

    public class QuestStageCheckView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _text;

        public void SetState(StageDisplayState state, string description, Sprite icon = null)
        {
            _icon.gameObject.SetActive(icon != null);
            if (icon != null) _icon.sprite = icon;
            _text.text = description ?? string.Empty;
        }
    }
}
