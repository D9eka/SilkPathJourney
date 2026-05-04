using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using Zenject;
using Plugins.Zenject.Source.Main;

namespace Internal.Scripts.UI.Screens.TargetSelection.Search
{
    public class MarketCityInfoView : MonoBehaviour, ILocalizationConsumer
    {
        [Header("Market Profile")]
        [SerializeField] private TextMeshProUGUI _purchaseLabel;
        [SerializeField] private LocalizedString _purchaseLocalized;
        [SerializeField] private TextMeshProUGUI _salesLabel;
        [SerializeField] private LocalizedString _salesLocalized;
        [SerializeField] private RectTransform _cheapToBuyContainer;
        [SerializeField] private RectTransform _expensiveToBuyContainer;
        [SerializeField] private RectTransform _cheapToSellContainer;
        [SerializeField] private RectTransform _expensiveToSellContainer;

        [Header("Icon Prefab")]
        [SerializeField] private ItemCategoryIconView _categoryIconPrefab;

        private readonly List<ItemCategoryIconView> _spawnedCheapBuy = new();
        private readonly List<ItemCategoryIconView> _spawnedExpBuy = new();
        private readonly List<ItemCategoryIconView> _spawnedCheapSell = new();
        private readonly List<ItemCategoryIconView> _spawnedExpSell = new();
        private LocalizationService _localization;
        private LocalizationService.LocalizedTextHandle _purchaseLabelHandle;
        private LocalizationService.LocalizedTextHandle _salesLabelHandle;

        [Inject] private DiContainer _container;

        public void SetLocalization(LocalizationService localization)
        {
            _localization = localization;
            BindRowLabels();
        }

        public void ApplyMarketProfile(CitySpecializationVm vm)
        {
            if (vm == null)
            {
                ClearProfile();
                return;
            }

            SpawnIcons(_spawnedCheapBuy, _cheapToBuyContainer, vm.CheapToBuy);
            SpawnIcons(_spawnedExpBuy, _expensiveToBuyContainer, vm.ExpensiveToBuy);
            SpawnIcons(_spawnedCheapSell, _cheapToSellContainer, vm.CheapToSell);
            SpawnIcons(_spawnedExpSell, _expensiveToSellContainer, vm.ExpensiveToSell);
        }

        private void SpawnIcons(List<ItemCategoryIconView> bucket, RectTransform parent, IReadOnlyList<ItemType> items)
        {
            foreach (ItemCategoryIconView view in bucket)
            {
                if (view != null)
                    Destroy(view.gameObject);
            }
            bucket.Clear();

            if (items == null) return;

            foreach (ItemType item in items)
            {
                ItemCategoryIconView instance = _container.InstantiatePrefabForComponent<ItemCategoryIconView>(_categoryIconPrefab, parent);
                instance.SetItemType(item);
                bucket.Add(instance);
            }
        }

        private void ClearProfile()
        {
            ClearBucket(_spawnedCheapBuy);
            ClearBucket(_spawnedExpBuy);
            ClearBucket(_spawnedCheapSell);
            ClearBucket(_spawnedExpSell);
        }

        private void ClearBucket(List<ItemCategoryIconView> bucket)
        {
            foreach (ItemCategoryIconView view in bucket)
            {
                if (view != null)
                    Destroy(view.gameObject);
            }
            bucket.Clear();
        }

        private void BindRowLabels()
        {
            if (_localization == null) return;

            _purchaseLabelHandle?.Dispose();
            _salesLabelHandle?.Dispose();

            if (_purchaseLabel != null && _purchaseLocalized != null)
                _purchaseLabelHandle = _localization.BindText(_purchaseLabel, _purchaseLocalized, "CityInfo.PurchaseLabel");
            if (_salesLabel != null && _salesLocalized != null)
                _salesLabelHandle = _localization.BindText(_salesLabel, _salesLocalized, "CityInfo.SalesLabel");
        }

        private void OnDestroy()
        {
            _purchaseLabelHandle?.Dispose();
            _purchaseLabelHandle = null;
            _salesLabelHandle?.Dispose();
            _salesLabelHandle = null;
        }
    }
}
