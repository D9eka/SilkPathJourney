using System;
using Internal.Scripts.UI.Screens.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.Workshop
{
    public class MainCartCardView : MonoBehaviour
    {
        [SerializeField] private CartStatsView _stats;
        [SerializeField] private TextMeshProUGUI _upgradeEffectText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TextMeshProUGUI _upgradeButtonText;

        private Action _onUpgrade;

        public void Initialize(MainCartViewData data, string upgradeButtonLabel, string maxUpgradeLabel, Action onUpgrade)
        {
            _onUpgrade = onUpgrade;

            if (_stats != null)
                _stats.Apply(data.Stats);

            if (_upgradeEffectText != null)
            {
                _upgradeEffectText.gameObject.SetActive(!data.IsMaxLevel);
                _upgradeEffectText.text = data.UpgradeEffectText;
            }

            if (_upgradeButtonText != null)
                _upgradeButtonText.text = data.IsMaxLevel ? maxUpgradeLabel : upgradeButtonLabel;

            if (_upgradeButton != null)
            {
                _upgradeButton.interactable = data.CanUpgrade;
                _upgradeButton.onClick.RemoveListener(HandleUpgrade);
                _upgradeButton.onClick.AddListener(HandleUpgrade);
            }
        }

        private void HandleUpgrade()
        {
            _onUpgrade?.Invoke();
        }

        private void OnDestroy()
        {
            if (_upgradeButton != null)
                _upgradeButton.onClick.RemoveListener(HandleUpgrade);
        }
    }
}
