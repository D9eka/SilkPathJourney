using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Screens.Core.ViewModel;
using Internal.Scripts.UI.Screens.Shared;
using Internal.Scripts.UI.Theme;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Caravansary
{
    public class CaravansaryScreen : PopupScreen
    {
        [Header("Indicators")]
        [SerializeField] private ResourceIndicator _moneyIndicator;
        [SerializeField] private ResourceIndicator _weightIndicator;

        [Header("Services")]
        [SerializeField] private OfferingPrefabMapping[] _offeringPrefabs;
        [SerializeField] private RectTransform _offeringContent;
        [SerializeField] private TextMeshProUGUI _servicesHeaderText;
        [SerializeField] private LocalizedString _servicesHeaderLocalized;

        [Header("Animals")]
        [SerializeField] private AnimalSwitchView _currentAnimalView;
        [SerializeField] private AnimalSwitchView _animalPrefab;
        [SerializeField] private RectTransform _animalContent;
        [SerializeField] private TextMeshProUGUI _animalsHeaderText;
        [SerializeField] private LocalizedString _animalsHeaderLocalized;
        [SerializeField] private TextMeshProUGUI _currentAnimalHeaderText;
        [SerializeField] private LocalizedString _currentAnimalHeaderLocalized;
        [SerializeField] private TextMeshProUGUI _availableAnimalsHeaderText;
        [SerializeField] private LocalizedString _availableAnimalsHeaderLocalized;

        private CaravansaryScreenViewModel _viewModel;
        private IDisposable _stateSubscription;
        private readonly List<AnimalSwitchView> _spawnedAnimals = new();
        private readonly List<GameObject> _spawnedOfferings = new();

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
            _viewModel = viewModel as CaravansaryScreenViewModel;
            SetupIcons();
            ResolveStaticTexts();
            SubscribeViewModel();
        }

        private void ResolveStaticTexts()
        {
            if (_servicesHeaderText != null)
                _servicesHeaderText.text = _servicesHeaderLocalized != null
                    ? LocalizationService.ResolveString(_servicesHeaderLocalized, "UI.Caravansary.Services.Header.Main", "Caravansary.ServicesHeader")
                    : "UI.Caravansary.Services.Header.Main";

            if (_animalsHeaderText != null)
                _animalsHeaderText.text = _animalsHeaderLocalized != null
                    ? LocalizationService.ResolveString(_animalsHeaderLocalized, "UI.Caravansary.Animals.Header.Main", "Caravansary.AnimalsHeader")
                    : "UI.Caravansary.Animals.Header.Main";

            if (_currentAnimalHeaderText != null)
                _currentAnimalHeaderText.text = _currentAnimalHeaderLocalized != null
                    ? LocalizationService.ResolveString(_currentAnimalHeaderLocalized, "UI.Caravansary.Animals.Header.Current", "Caravansary.CurrentAnimalHeader")
                    : "UI.Caravansary.Animals.Header.Current";

            if (_availableAnimalsHeaderText != null)
                _availableAnimalsHeaderText.text = _availableAnimalsHeaderLocalized != null
                    ? LocalizationService.ResolveString(_availableAnimalsHeaderLocalized, "UI.Caravansary.Animals.Header.Available", "Caravansary.AvailableAnimalsHeader")
                    : "UI.Caravansary.Animals.Header.Available";
        }

        private void SetupIcons()
        {
            var icons = _viewModel?.ResourceIcons;
            if (icons == null) return;
            _moneyIndicator?.SetIcon(icons.Get(ResourceType.Money)?.Icon);
            _weightIndicator?.SetIcon(icons.Get(ResourceType.Weight)?.Icon);
        }

        private void SubscribeViewModel()
        {
            if (_viewModel == null || _stateSubscription != null)
                return;

            _stateSubscription = _viewModel.State.Subscribe(ApplyState);
        }

        private void UnsubscribeViewModel()
        {
            if (_viewModel == null)
                return;

            _stateSubscription?.Dispose();
            _stateSubscription = null;
        }

        private void ApplyState(CaravansaryViewState state)
        {
            if (state == null) return;

            _moneyIndicator?.SetValue(state.PlayerMoney);
            _weightIndicator?.SetValue(state.WeightFormatted);

            RebuildOfferings(state.Offerings);
            ApplyCurrentAnimal(state.CurrentAnimal);
            RebuildAnimals(state.AvailableAnimals);
        }

        private void RebuildOfferings(IReadOnlyList<OfferingItem> offerings)
        {
            foreach (var item in _spawnedOfferings)
                Destroy(item);
            _spawnedOfferings.Clear();

            if (offerings == null || _offeringContent == null) return;

            foreach (var offering in offerings)
            {
                var prefab = GetOfferingPrefab(offering.Type);
                if (prefab == null) continue;

                var instance = Instantiate(prefab, _offeringContent);
                instance.InitializeColorBinders(themeService: _viewModel?.ThemeService);

                var view = instance.GetComponent<IOfferingItemView>();
                if (view == null) continue;

                int index = offering.Index;
                view.Initialize(offering,
                    () => _viewModel?.ExecuteOffering(index, false),
                    () => _viewModel?.ExecuteOffering(index, true));

                _spawnedOfferings.Add(instance);
            }
        }

        private GameObject GetOfferingPrefab(OfferingType type)
        {
            if (_offeringPrefabs == null) return null;
            foreach (var mapping in _offeringPrefabs)
            {
                if (mapping.Type == type)
                    return mapping.Prefab;
            }
            return null;
        }

        private void ApplyCurrentAnimal(AnimalSwitchData current)
        {
            if (_currentAnimalView != null)
                _currentAnimalView.Initialize(current, HandleSwitchAnimal);
        }

        private void RebuildAnimals(IReadOnlyList<AnimalSwitchData> animals)
        {
            foreach (var item in _spawnedAnimals)
                Destroy(item.gameObject);
            _spawnedAnimals.Clear();

            if (animals == null || _animalPrefab == null || _animalContent == null)
                return;

            foreach (var animal in animals)
            {
                var instance = Instantiate(_animalPrefab, _animalContent);
                instance.gameObject.InitializeColorBinders(themeService: _viewModel?.ThemeService);
                instance.Initialize(animal, HandleSwitchAnimal);
                _spawnedAnimals.Add(instance);
            }
        }

        private void HandleSwitchAnimal(DraftAnimalType animalType)
        {
            _viewModel?.SwitchAnimal(animalType);
        }
    }

    [Serializable]
    public struct OfferingPrefabMapping
    {
        public OfferingType Type;
        public GameObject Prefab;
    }
}
