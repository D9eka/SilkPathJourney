using Internal.Scripts.UI.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Screens.Legacy.Tabs.LifetimeStats.Sections
{
    public abstract class LifeStatsSection : MonoBehaviour
    {
        private const string MissingValueFallback = "-";

        [SerializeField] protected TextMeshProUGUI _header;

        protected LocalizationService.LocalizedTextHandle HeaderHandle;

        public virtual void BindLocalization(LocalizationService localization)
        {
            DisposeBinding();
        }

        public virtual void DisposeBinding()
        {
            HeaderHandle?.Dispose();
            HeaderHandle = null;
        }

        public abstract void Apply(LifetimeStatsViewState state);

        protected static LocalizationService.LocalizedTextHandle BindValue(
            TextMeshProUGUI target, string key, LocalizationService localization)
        {
            if (target == null || localization == null) return null;

            return localization.BindText(
                target,
                new LocalizedString("UI", key),
                $"{target.name}.{key}",
                MissingValueFallback,
                null,
                MissingValueFallback);
        }

        protected static void SetArg(LocalizationService.LocalizedTextHandle handle, int value)
            => handle?.SetArguments(value.ToString(), value);

        protected static void SetArg(LocalizationService.LocalizedTextHandle handle, string value)
            => handle?.SetArguments(value, value);

        protected static void SetArg(LocalizationService.LocalizedTextHandle handle, int a, int b)
            => handle?.SetArguments($"{a} / +{b}", a, b);
    }
}
