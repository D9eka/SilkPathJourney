using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
namespace Internal.Scripts.UI.Screens.Legacy.Tabs.Achievement
{
    public sealed class AchievementEntryView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private IconView _statusIcon;

        [Header("Progress")]
        [SerializeField] private TextMeshProUGUI _requirementsText;
        [SerializeField] private TextMeshProUGUI _progressLabel;
        [SerializeField] private TextMeshProUGUI _progressValueText;
        [SerializeField] private FillBar _progressFill;
        [SerializeField] private GameObject _progressGroup;

        [Header("Earned Date")]
        [SerializeField] private TextMeshProUGUI _earnedDateText;
        [SerializeField] private GameObject _earnedDateGroup;

        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _progressHandle;
        private LocalizationService.LocalizedTextHandle _requirementsHandle;
        private LocalizationService.LocalizedTextHandle _earnedDateHandle;

        private LocalizedString _requirementsLocalized;
        private LocalizedString _earnedDateLocalized;
        private LocalizedString _earnedLocalized;
        private LocalizedString _progressLocalized;

        private void Awake()
        {
            _requirementsLocalized = new LocalizedString("UI", LocUI.UI_LegacyShop_Achievement_Requirements);
            _earnedDateLocalized = new LocalizedString("UI", LocUI.UI_LegacyShop_Achievement_EarnedDate);
            _earnedLocalized = new LocalizedString("UI", LocUI.UI_LegacyShop_Achievement_Earned);
            _progressLocalized = new LocalizedString("UI", LocUI.UI_LegacyShop_Achievement_Progress);
        }

        public void BindLocalization(LocalizationService localization)
        {
            _localization = localization;

            _progressHandle?.Dispose();
            _progressHandle = localization.BindText(
                _progressLabel,
                _progressLocalized,
                $"{name}.Progress");
        }

        public void DisposeBinding()
        {
            _progressHandle?.Dispose();
            _progressHandle = null;
            _requirementsHandle?.Dispose();
            _requirementsHandle = null;
            _earnedDateHandle?.Dispose();
            _earnedDateHandle = null;
        }

        private void OnDestroy() => DisposeBinding();

        public void Apply(AchievementEntryData data)
        {
            _nameText.text = data.Name;
            _rewardText.text = $"+{data.LegacyReward}";
            _statusIcon.gameObject.SetActive(data.IsEarned);

            _requirementsHandle?.Dispose();
            _requirementsHandle = _localization?.BindText(
                _requirementsText, _requirementsLocalized,
                $"{name}.Requirements", string.Empty,
                null, data.Description ?? string.Empty);

            bool hasProgress = !data.IsEarned && data.TargetValue > 0;
            bool hasDate = data.IsEarned && !string.IsNullOrEmpty(data.EarnedDate);

            _progressGroup?.SetActive(hasProgress);
            _earnedDateGroup?.SetActive(data.IsEarned);

            if (hasProgress)
            {
                _progressValueText.text = $"{data.CurrentProgress} / {data.TargetValue}";
                _progressFill.SetFill(data.TargetValue > 0
                    ? Mathf.Clamp01((float)data.CurrentProgress / data.TargetValue)
                    : 0f);
            }

            _earnedDateHandle?.Dispose();
            _earnedDateHandle = null;

            if (data.IsEarned && _localization != null)
            {
                if (hasDate)
                    _earnedDateHandle = _localization.BindText(_earnedDateText,
                        _earnedDateLocalized,
                        $"{name}.EarnedDate", string.Empty,
                        null, data.EarnedDate);
                else
                    _earnedDateHandle = _localization.BindText(_earnedDateText,
                        _earnedLocalized,
                        $"{name}.Earned");
            }
        }
    }
}
