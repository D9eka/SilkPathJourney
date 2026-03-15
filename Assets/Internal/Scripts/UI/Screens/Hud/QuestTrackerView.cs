using DG.Tweening;
using Internal.Scripts.Quests.Data;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Quests;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

using static Internal.Scripts.UI.Screens.Quests.QuestLocContext;

namespace Internal.Scripts.UI.Screens.Hud
{
    public class QuestTrackerView : MonoBehaviour
    {
        [Header("Quest Info")]
        [SerializeField] private TextMeshProUGUI _questNameText;
        [SerializeField] private Image _questIcon;
        [SerializeField] private Button _openQuestsButton;

        [Header("Stages")]
        [SerializeField] private QuestStageCheckView _currentStage;
        [SerializeField] private QuestStageCheckView _nextStage;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _root;

        private const float FADE_DURATION = 0.25f;
        private const float SHOW_DURATION = 0.3f;
        private const float STAGE_SLIDE_DURATION = 0.25f;
        private const float STRIKETHROUGH_DELAY = 0.3f;
        private const float STAGE_GAP_DELAY = 0.15f;
        private const float COMPLETE_DURATION = 0.4f;
        private const float INITIAL_SCALE = 0.8f;
        private const float SLIDE_OFFSET_Y = 20f;
        private const float COMPLETED_STAGE_ALPHA = 0.3f;

        private Sequence _sequence;

        public Button OpenQuestsButton => _openQuestsButton;

        public void ApplyState(QuestTrackerState state)
        {
            switch (state.ChangeType)
            {
                case TrackerChangeType.NewQuest:
                    AnimateShow(state.Quest, state.CurrentStageIndex);
                    break;
                case TrackerChangeType.StageAdvanced:
                    AnimateStageAdvance(state.Quest, state.PreviousStageIndex, state.CurrentStageIndex);
                    break;
                case TrackerChangeType.QuestCompleted:
                    AnimateQuestComplete();
                    break;
                case TrackerChangeType.QuestFailed:
                    AnimateQuestFailed(state.Quest);
                    break;
                case TrackerChangeType.QuestSwitched:
                    AnimateSwitchQuest(state.Quest, state.CurrentStageIndex);
                    break;
                case TrackerChangeType.None:
                    SetQuestImmediate(state.Quest, state.CurrentStageIndex);
                    break;
            }
        }

        public void Hide()
        {
            KillSequence();

            if (_canvasGroup == null || !gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                return;
            }

            _sequence = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(0f, FADE_DURATION).SetEase(Ease.InCubic))
                .OnComplete(() => gameObject.SetActive(false))
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        private void AnimateShow(QuestData quest, int stageIndex)
        {
            KillSequence();
            SetQuestData(quest, stageIndex);
            gameObject.SetActive(true);

            if (_canvasGroup == null)
                return;

            _canvasGroup.alpha = 0f;
            if (_root != null) _root.localScale = Vector3.one * INITIAL_SCALE;

            _sequence = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(1f, SHOW_DURATION).SetEase(Ease.OutCubic));

            if (_root != null)
                _sequence.Join(_root.DOScale(1f, SHOW_DURATION).SetEase(Ease.OutCubic));

            _sequence.SetLink(gameObject).SetUpdate(true);
        }

        private void AnimateStageAdvance(QuestData quest, int previousIndex, int newIndex)
        {
            KillSequence();

            if (_currentStage == null)
            {
                SetQuestData(quest, newIndex);
                return;
            }

            var currentRT = _currentStage.GetComponent<RectTransform>();
            var currentCG = _currentStage.GetComponent<CanvasGroup>();

            string prevDesc = GetStageDescription(quest, previousIndex);
            _currentStage.SetState(StageDisplayState.Completed, $"<s>{prevDesc}</s>");

            _sequence = DOTween.Sequence();

            if (currentCG != null)
                _sequence.Append(currentCG.DOFade(COMPLETED_STAGE_ALPHA, STRIKETHROUGH_DELAY).SetEase(Ease.OutCubic));
            else
                _sequence.AppendInterval(STRIKETHROUGH_DELAY);

            _sequence.AppendInterval(STAGE_GAP_DELAY);

            if (currentCG != null && currentRT != null)
            {
                _sequence.Append(currentCG.DOFade(0f, STAGE_SLIDE_DURATION).SetEase(Ease.InCubic));
                _sequence.Join(currentRT.DOAnchorPosY(
                    currentRT.anchoredPosition.y + SLIDE_OFFSET_Y, STAGE_SLIDE_DURATION).SetEase(Ease.InCubic));
            }

            _sequence.AppendCallback(() =>
            {
                if (currentRT != null) currentRT.anchoredPosition = Vector2.zero;
                if (currentCG != null) currentCG.alpha = 1f;
                SetStages(quest, newIndex);
            });

            if (currentCG != null)
            {
                _sequence.Append(currentCG.DOFade(1f, FADE_DURATION).SetEase(Ease.OutCubic));
            }

            _sequence.SetLink(gameObject).SetUpdate(true);
        }

        private void AnimateQuestComplete()
        {
            KillSequence();

            if (_canvasGroup == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _sequence = DOTween.Sequence();

            if (_root != null)
                _sequence.Append(_root.DOScale(0f, COMPLETE_DURATION).SetEase(Ease.InCubic));

            _sequence.Join(_canvasGroup.DOFade(0f, COMPLETE_DURATION).SetEase(Ease.InCubic));

            _sequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                if (_root != null) _root.localScale = Vector3.one;
                _canvasGroup.alpha = 1f;
            });

            _sequence.SetLink(gameObject).SetUpdate(true);
        }

        private void AnimateQuestFailed(QuestData quest)
        {
            KillSequence();
            gameObject.SetActive(true);

            if (quest != null && _questNameText != null)
                _questNameText.text = $"<s>{LocalizationService.ResolveString(quest.Name, quest.Id, QuestName(quest.Id))}</s>";

            if (_currentStage != null)
            {
                _currentStage.gameObject.SetActive(true);
                _currentStage.SetState(StageDisplayState.Completed, LocalizationService.ResolveString(new LocalizedString("UI", "UI.Quest.Failed"), "Quest failed", "QuestTracker"));
            }

            if (_nextStage != null)
                _nextStage.gameObject.SetActive(false);

            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            if (_root != null) _root.localScale = Vector3.one;

            _sequence = DOTween.Sequence()
                .AppendInterval(3f)
                .Append(_canvasGroup != null
                    ? _canvasGroup.DOFade(0f, COMPLETE_DURATION).SetEase(Ease.InCubic)
                    : DOTween.Sequence().AppendInterval(0f))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    if (_canvasGroup != null) _canvasGroup.alpha = 1f;
                })
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        private void AnimateSwitchQuest(QuestData quest, int stageIndex)
        {
            KillSequence();

            if (_canvasGroup == null)
            {
                SetQuestData(quest, stageIndex);
                return;
            }

            _sequence = DOTween.Sequence()
                .Append(_canvasGroup.DOFade(0f, FADE_DURATION).SetEase(Ease.InCubic))
                .AppendCallback(() => SetQuestData(quest, stageIndex))
                .Append(_canvasGroup.DOFade(1f, SHOW_DURATION).SetEase(Ease.OutCubic))
                .SetLink(gameObject)
                .SetUpdate(true);
        }

        private void SetQuestImmediate(QuestData quest, int stageIndex)
        {
            KillSequence();

            if (quest == null)
            {
                gameObject.SetActive(false);
                return;
            }

            SetQuestData(quest, stageIndex);
            gameObject.SetActive(true);

            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            if (_root != null) _root.localScale = Vector3.one;
        }

        private void SetQuestData(QuestData quest, int stageIndex)
        {
            if (_questNameText != null)
                _questNameText.text = LocalizationService.ResolveString(
                    quest.Name, quest.Id, QuestName(quest.Id));

            if (_questIcon != null)
                _questIcon.sprite = quest.Icon;

            SetStages(quest, stageIndex);
        }

        private void SetStages(QuestData quest, int stageIndex)
        {
            if (_currentStage != null)
            {
                string desc = GetStageDescription(quest, stageIndex);
                _currentStage.gameObject.SetActive(!string.IsNullOrEmpty(desc));
                _currentStage.SetState(StageDisplayState.Current, desc);
            }

            if (_nextStage != null)
            {
                int nextIndex = stageIndex + 1;
                string nextDesc = GetStageDescription(quest, nextIndex);
                _nextStage.gameObject.SetActive(!string.IsNullOrEmpty(nextDesc));
                _nextStage.SetState(StageDisplayState.Future, nextDesc);
            }
        }

        private static string GetStageDescription(QuestData quest, int index)
        {
            if (quest.Stages == null || index < 0 || index >= quest.Stages.Count)
                return string.Empty;

            var stage = quest.Stages[index];
            return LocalizationService.ResolveString(
                stage.Description, stage.Id, StageDesc(quest.Id, stage.Id));
        }

        private void KillSequence()
        {
            if (_sequence != null && _sequence.IsActive())
                _sequence.Kill();
            _sequence = null;
        }

        private void OnDestroy()
        {
            KillSequence();
        }
    }
}
