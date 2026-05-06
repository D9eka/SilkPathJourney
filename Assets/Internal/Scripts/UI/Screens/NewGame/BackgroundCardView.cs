using Internal.Scripts.Player.Background;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.NewGame
{
    public class BackgroundCardView : MonoBehaviour
    {
        [SerializeField] private Button _clickArea;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Common (visible in both states)")]
        [SerializeField] private TextMeshProUGUI _roleText;
        [SerializeField] private LocalizedString _roleLabelString;

        [Header("Content (unlocked)")]
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private LabeledBarView _difficultyBar;
        [SerializeField] private LocalizedString _difficultyLabelString;
        [SerializeField] private LabeledBarView _mainSkillBar;
        [SerializeField] private LabeledBarView _secondarySkillBar;
        [SerializeField] private TextMeshProUGUI _featuresText;
        [SerializeField] private Button _selectButton;
        [SerializeField] private TextMeshProUGUI _selectButtonText;
        [SerializeField] private LocalizedString _selectButtonString;
        [SerializeField] private GameObject _recommendedRoot;
        [SerializeField] private TextMeshProUGUI _recommendedText;
        [SerializeField] private LocalizedString _recommendedString;

        [Header("Locked")]
        [SerializeField] private GameObject _lockedRoot;
        [SerializeField] private IconLabel _lockedLabel;
        [SerializeField] private LocalizedString _lockedFormatString;
        [SerializeField] private TextMeshProUGUI _lockedDescriptionText;
        [SerializeField] private LocalizedString _lockedDescriptionString;

        [Header("Selection")]
        [SerializeField] private GameObject _selectedBorder;

        private BackgroundData _data;
        private NewGameScreenViewModel _viewModel;

        private LocalizationService.LocalizedTextHandle _roleHandle;
        private LocalizationService.LocalizedTextHandle _selectButtonHandle;
        private LocalizationService.LocalizedTextHandle _recommendedHandle;
        private LocalizationService.LocalizedTextHandle _lockedHandle;
        private LocalizationService.LocalizedTextHandle _lockedDescriptionHandle;

        public BackgroundData Data => _data;

        public void SetData(BackgroundData data, NewGameScreenViewModel viewModel, bool isSelected)
        {
            _data = data;
            _viewModel = viewModel;

            bool unlocked = viewModel.IsBackgroundUnlocked(data);
            LocalizationService loc = viewModel.Localization;

            _titleText.text = LocalizationService.ResolveString(data.Name, data.Id, $"Background.{data.Id}.Name");

            BindRoleText(loc, data);

            _contentRoot.SetActive(unlocked);
            _lockedRoot.SetActive(!unlocked);

            if (unlocked)
                BindContent(data, viewModel);
            else
                BindLocked(data, viewModel);

            _selectedBorder.SetActive(isSelected);

            _clickArea.interactable = unlocked;
            _clickArea.onClick.RemoveAllListeners();
            if (unlocked)
                _clickArea.onClick.AddListener(OnClick);
        }

        public void SetSelected(bool selected)
        {
            _selectedBorder.SetActive(selected);
        }

        private void BindContent(BackgroundData data, NewGameScreenViewModel viewModel)
        {
            LocalizationService loc = viewModel.Localization;

            _difficultyBar.Initialize(loc, _difficultyLabelString, null, data.GetDifficultyFill(), (string)null);

            LocalizedString mainSkillName = SkillView.ResolveSkillName(data.MainSkillType);
            float mainProgress = (float)data.GetSkillValue(data.MainSkillType) / BackgroundData.MaxSkillValue;
            _mainSkillBar.Initialize(loc, mainSkillName, null, mainProgress, (string)null);

            LocalizedString secondarySkillName = SkillView.ResolveSkillName(data.SecondarySkillType);
            float secondaryProgress = (float)data.GetSkillValue(data.SecondarySkillType) / BackgroundData.MaxSkillValue;
            _secondarySkillBar.Initialize(loc, secondarySkillName, null, secondaryProgress, (string)null);

            _featuresText.text = string.IsNullOrEmpty(data.FeaturesKey)
                ? string.Empty
                : LocalizationService.Resolve("Player", data.FeaturesKey);

            _selectButtonHandle?.Dispose();
            _selectButtonHandle = loc.BindText(_selectButtonText, _selectButtonString, $"{name}.SelectButton");
            _selectButton.onClick.RemoveAllListeners();
            _selectButton.onClick.AddListener(OnClick);

            _recommendedRoot.SetActive(data.IsRecommended);
            if (data.IsRecommended)
            {
                _recommendedHandle?.Dispose();
                _recommendedHandle = loc.BindText(_recommendedText, _recommendedString, $"{name}.Recommended");
            }
        }

        private void BindRoleText(LocalizationService loc, BackgroundData data)
        {
            _roleHandle?.Dispose();
            string roleName = string.IsNullOrEmpty(data.RoleKey)
                ? string.Empty
                : LocalizationService.Resolve("Player", data.RoleKey);
            _roleHandle = loc.BindText(_roleText, _roleLabelString, $"{name}.RoleLabel",
                postProcess: prefix => string.IsNullOrEmpty(roleName) ? prefix : $"{prefix} {roleName}");
        }

        private void BindLocked(BackgroundData data, NewGameScreenViewModel viewModel)
        {
            Sprite lockIcon = viewModel.IconCatalog?.Get(ResourceType.Lock)?.Icon;
            _lockedLabel.Initialize(lockIcon, null);
            LocalizationService loc = viewModel.Localization;
            string emptyFallback = string.Empty;

            _lockedHandle?.Dispose();
            TextMeshProUGUI lockedTmp = GetLockedLabelText();
            if (lockedTmp != null)
                _lockedHandle = loc.BindText(lockedTmp, _lockedFormatString, $"{name}.Locked");

            _lockedDescriptionHandle?.Dispose();
            if (_lockedDescriptionText != null && _lockedDescriptionString != null)
                _lockedDescriptionHandle = loc.BindText(_lockedDescriptionText, _lockedDescriptionString,
                    $"{name}.LockedDescription", emptyFallback, postProcess: null,
                    args: new object[] { data.UnlockCost });
        }

        private TextMeshProUGUI GetLockedLabelText() => _lockedLabel != null
            ? _lockedLabel.GetComponentInChildren<TextMeshProUGUI>()
            : null;

        private void OnDisable()
        {
            _roleHandle?.Dispose();
            _selectButtonHandle?.Dispose();
            _recommendedHandle?.Dispose();
            _lockedHandle?.Dispose();
            _lockedDescriptionHandle?.Dispose();
            _roleHandle = null;
            _selectButtonHandle = null;
            _recommendedHandle = null;
            _lockedHandle = null;
            _lockedDescriptionHandle = null;
        }

        private void OnClick()
        {
            if (_viewModel != null && _data != null)
                _viewModel.SelectedBackground.Value = _data;
        }
    }
}
