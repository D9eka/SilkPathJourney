using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Guild;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Shared;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Guild
{
    public class GuildScreen : PopupScreen
    {
        [Header("Header")]
        [SerializeField] private ResourceIndicator _moneyIndicator;
        [SerializeField] private ResourceIndicator _weightIndicator;

        [Header("Offerings")]
        [SerializeField] private TextMeshProUGUI _offeringsHeaderText;
        [SerializeField] private LocalizedString _offeringsHeaderLocalized;
        [SerializeField] private ServiceCardView _offeringPrefab;
        [SerializeField] private RectTransform _offeringsContent;

        [Header("Contracts")]
        [SerializeField] private TextMeshProUGUI _contractsHeaderText;
        [SerializeField] private LocalizedString _contractsHeaderLocalized;
        [SerializeField] private ServiceCardView _contractPrefab;
        [SerializeField] private RectTransform _contractContent;

        [Header("Quest (placeholder — guild quest system WIP)")]
        [SerializeField] private QuestCardView _questView;

        [Header("Empty State")]
        [SerializeField] private GameObject _contractsEmptyContainer;
        [SerializeField] private TextMeshProUGUI _contractsEmptyText;
        [SerializeField] private LocalizedString _contractsEmptyNotMemberLoc;
        [SerializeField] private LocalizedString _contractsEmptyNoContractsLoc;

        [Header("Contracts — localization")]
        [SerializeField] private LocalizedString _contractCourierTitleLoc;
        [SerializeField] private LocalizedString _contractCargoTitleLoc;
        [SerializeField] private LocalizedString _contractAcceptButtonLoc;
        [SerializeField] private LocalizedString _contractCourierDescLoc;
        [SerializeField] private LocalizedString _contractCargoDescLoc;
        [SerializeField] private LocalizedString _activeContractTitleLoc;
        [SerializeField] private LocalizedString _activeContractCompleteButtonLoc;

        private readonly List<ServiceCardView> _spawnedOfferings = new();
        private readonly List<ServiceCardView> _spawnedContracts = new();

        private GuildScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private GuildViewState _currentState;

        protected override void OnEnable()
        {
            base.OnEnable();
            SubscribeViewModel();
        }

        protected override void OnDisable()
        {
            UnsubscribeViewModel();
            base.OnDisable();
        }

        public override void BindViewModel(IScreenViewModel viewModel)
        {
            _viewModel = viewModel as GuildScreenViewModel;
            SubscribeViewModel();
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
        }

        private void UnsubscribeViewModel()
        {
            _stateSubscription?.Dispose();
            _stateSubscription = null;
        }

        private void ApplyState(GuildViewState state)
        {
            if (state == null) return;
            _currentState = state;

            _moneyIndicator.SetResource(state.MoneyIcon, state.PlayerMoney);
            ApplyWeight(state);

            _offeringsHeaderText.text = LocalizationService.ResolveString(
                _offeringsHeaderLocalized, "Услуги", "Guild.OfferingsHeader");
            _contractsHeaderText.text = LocalizationService.ResolveString(
                _contractsHeaderLocalized, "Контракты", "Guild.ContractsHeader");

            _questView.Hide();

            RebuildOfferings(state.Offerings);
            RebuildContracts(state);
        }

        private void ApplyWeight(GuildViewState state)
        {
            _weightIndicator.SetIcon(state.WeightIcon);
            string weightText = state.MaxWeight > 0f
                ? $"{state.BaseWeight:0.##} / {state.MaxWeight:0.##}"
                : $"{state.BaseWeight:0.##}";
            _weightIndicator.SetValue(weightText);
            _weightIndicator.HideChangeImmediate();
            _weightIndicator.SetHighlight(state.BaseWeight > state.MaxWeight);
        }

        private void ShowProjectedWeight(float addWeight)
        {
            if (_currentState == null || addWeight <= 0f) return;

            float projected = _currentState.BaseWeight + addWeight;
            string weightText = _currentState.MaxWeight > 0f
                ? $"{projected:0.##} / {_currentState.MaxWeight:0.##}"
                : $"{projected:0.##}";
            _weightIndicator.SetValue(weightText);
            _weightIndicator.SetChange(addWeight, increaseIsPositive: false);
            _weightIndicator.SetHighlight(projected > _currentState.MaxWeight);
        }

        private void ResetWeight()
        {
            if (_currentState == null) return;
            ApplyWeight(_currentState);
        }

        private void RebuildOfferings(OfferingItem[] offerings)
        {
            foreach (var card in _spawnedOfferings)
                Destroy(card.gameObject);
            _spawnedOfferings.Clear();

            if (offerings == null) return;

            foreach (var item in offerings)
            {
                if (!item.IsVisible) continue;
                var instance = Instantiate(_offeringPrefab, _offeringsContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                int idx = item.Index;
                instance.Initialize(item, () => _viewModel?.TriggerOffering(idx), null);
                _spawnedOfferings.Add(instance);
            }
        }

        private void RebuildContracts(GuildViewState state)
        {
            foreach (var card in _spawnedContracts)
                Destroy(card.gameObject);
            _spawnedContracts.Clear();

            if (state.HasActiveContract)
                SpawnActiveContract(state.ActiveContract);

            var contracts = state.AvailableContracts;
            bool hasAvailable = contracts != null && contracts.Length > 0;

            if (hasAvailable)
            {
                for (int i = 0; i < contracts.Length; i++)
                    SpawnAvailableContract(contracts[i], i, state.HasActiveContract);
            }

            ApplyContractsEmptyState(state, hasAvailable);
        }

        private void ApplyContractsEmptyState(GuildViewState state, bool hasAvailable)
        {
            bool showEmpty = !state.HasActiveContract && !hasAvailable;
            _contractsEmptyContainer.SetActive(showEmpty);
            if (!showEmpty) return;

            _contractsEmptyText.text = state.IsMember
                ? LocalizationService.ResolveString(_contractsEmptyNoContractsLoc,
                    "Нет доступных контрактов", "Guild.Contracts.Empty.NoContracts")
                : LocalizationService.ResolveString(_contractsEmptyNotMemberLoc,
                    "Вступите в гильдию, чтобы получить доступ к контрактам",
                    "Guild.Contracts.Empty.NotMember");
        }

        private void SpawnActiveContract(GuildContractEntry contract)
        {
            var instance = Instantiate(_contractPrefab, _contractContent);
            instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);

            string title = LocalizationService.ResolveString(_activeContractTitleLoc,
                $"Активный: Доставка в {contract.TargetCityName}",
                "Guild.ActiveContract.Title", contract.TargetCityName);

            string description = BuildContractDescription(contract);

            string buttonText = LocalizationService.ResolveString(_activeContractCompleteButtonLoc,
                "Сдать", "Guild.ActiveContract.Complete.Button");

            instance.Initialize(title, description, buttonText, true,
                () => _viewModel?.TryCompleteContract());
            _spawnedContracts.Add(instance);
        }

        private void SpawnAvailableContract(GuildContractEntry contract, int index, bool hasActive)
        {
            var instance = Instantiate(_contractPrefab, _contractContent);
            instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
            int idx = index;

            string title = BuildContractTitle(contract);
            string description = BuildContractDescription(contract);

            string buttonText = LocalizationService.ResolveString(_contractAcceptButtonLoc,
                "Принять", "Guild.Contract.Accept.Button");

            instance.Initialize(title, description, buttonText, !hasActive,
                () => _viewModel?.AcceptContract(idx));

            AttachHover(instance.gameObject, contract.CargoWeightKg);

            _spawnedContracts.Add(instance);
        }

        private void AttachHover(GameObject cardGameObject, float cargoWeight)
        {
            if (cargoWeight <= 0f) return;

            var hover = HoverReporter.GetOrAdd(cardGameObject);
            hover.Entered += () => ShowProjectedWeight(cargoWeight);
            hover.Exited += ResetWeight;
        }

        private string BuildContractTitle(GuildContractEntry contract)
        {
            return contract.ContractType == GuildContractType.Cargo
                ? LocalizationService.ResolveString(_contractCargoTitleLoc,
                    $"Грузоперевозка в {contract.TargetCityName}",
                    "Guild.Contract.Cargo.Title", contract.TargetCityName)
                : LocalizationService.ResolveString(_contractCourierTitleLoc,
                    $"Курьерская доставка в {contract.TargetCityName}",
                    "Guild.Contract.Courier.Title", contract.TargetCityName);
        }

        private string BuildContractDescription(GuildContractEntry contract)
        {
            return contract.ContractType == GuildContractType.Cargo
                ? LocalizationService.ResolveString(_contractCargoDescLoc,
                    $"Вес: {contract.CargoWeightKg}кг · {contract.DaysToDeliver} дн. · Награда: {contract.Reward}",
                    "Guild.Contract.Cargo.Desc",
                    contract.CargoWeightKg, contract.DaysToDeliver, contract.Reward)
                : LocalizationService.ResolveString(_contractCourierDescLoc,
                    $"Награда: {contract.Reward} · За {contract.DaysToDeliver} дн.",
                    "Guild.Contract.Courier.Desc",
                    contract.Reward, contract.DaysToDeliver);
        }
    }
}
