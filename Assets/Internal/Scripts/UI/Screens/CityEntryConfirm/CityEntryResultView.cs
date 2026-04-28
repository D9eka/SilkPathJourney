using System;
using System.Text;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.CityEntryConfirm
{
    public class CityEntryResultView : MonoBehaviour
    {
        [Header("Duty")]
        [SerializeField] private IconLabel _dutyPaidView;

        [Header("Smuggling Outcome")]
        [SerializeField] private TextMeshProUGUI _resultOutcomeText;
        [SerializeField] private TextMeshProUGUI _consequencesText;

        [Header("Buttons")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private TextMeshProUGUI _continueButtonText;

        [Header("Localization")]
        [SerializeField] private LocalizedString _dutyPaidLocalized;
        [SerializeField] private LocalizedString _smugglingPassedLocalized;
        [SerializeField] private LocalizedString _smugglingCaughtLocalized;
        [SerializeField] private LocalizedString _confiscatedLocalized;
        [SerializeField] private LocalizedString _fineLocalized;
        [SerializeField] private LocalizedString _continueButtonLocalized;

        public event Action ContinueClicked;

        private void Awake()
        {
            _continueButton.onClick.AddListener(() => ContinueClicked?.Invoke());
        }

        public void Apply(CityEntryConfirmViewState state)
        {
            _dutyPaidView.Initialize(state.MoneyIcon, LocalizationService.ResolveString(
                _dutyPaidLocalized, $"Пошлина уплачена: {state.TariffResult.Amount}",
                "CityEntryConfirm.DutyPaid", state.TariffResult.Amount));

            ApplySmugglingOutcome(state);
            ApplyConsequences(state);

            _continueButtonText.text = LocalizationService.ResolveString(
                _continueButtonLocalized, "Продолжить", "CityEntryConfirm.ContinueButton");
        }

        private void ApplySmugglingOutcome(CityEntryConfirmViewState state)
        {
            _resultOutcomeText.gameObject.SetActive(state.SmugglingResult.WasChecked);
            if (!state.SmugglingResult.WasChecked) return;

            if (state.SmugglingResult.WasCaught)
            {
                _resultOutcomeText.text = LocalizationService.ResolveString(
                    _smugglingCaughtLocalized, "Контрабанда обнаружена!",
                    "CityEntryConfirm.Caught");
            }
            else
            {
                _resultOutcomeText.text = LocalizationService.ResolveString(
                    _smugglingPassedLocalized, "Контрабанда не обнаружена",
                    "CityEntryConfirm.Passed");
            }
        }

        private void ApplyConsequences(CityEntryConfirmViewState state)
        {
            var result = state.SmugglingResult;
            bool hasConsequences = result.WasChecked && result.WasCaught
                                && (result.ConfiscatedValue > 0 || result.Penalty > 0);

            _consequencesText.gameObject.SetActive(hasConsequences);
            if (!hasConsequences) return;

            var sb = new StringBuilder();
            if (result.ConfiscatedValue > 0)
                sb.Append(LocalizationService.ResolveString(
                    _confiscatedLocalized, $"Конфисковано на {result.ConfiscatedValue}",
                    "CityEntryConfirm.Confiscated", result.ConfiscatedValue));

            if (result.Penalty > 0)
            {
                if (sb.Length > 0) sb.Append('\n');
                sb.Append(LocalizationService.ResolveString(
                    _fineLocalized, $"Штраф: {result.Penalty}",
                    "CityEntryConfirm.Fine", result.Penalty));
            }

            _consequencesText.text = sb.ToString();
        }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveAllListeners();
        }
    }
}
