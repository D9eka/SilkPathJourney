using System;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Internal.Scripts.UI.Screens.HazardQte
{
    public sealed class HazardTimerView : IDisposable
    {
        private readonly FillBar _bar;
        private readonly TextMeshProUGUI _text;

        private string _suffix;
        private float _lastRemaining;
        private float _lastTotal;

        public HazardTimerView(FillBar bar, TextMeshProUGUI text)
        {
            _bar = bar;
            _text = text;
            RefreshSuffix();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        }

        public void Render(float remaining, float total)
        {
            _lastRemaining = remaining;
            _lastTotal = total;
            _bar.SetFill(total > 0f ? remaining / total : 0f);
            _text.text = $"{Mathf.Max(0f, remaining):F2}{_suffix}";
        }

        public void Dispose() => LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;

        private void OnLocaleChanged(Locale _)
        {
            RefreshSuffix();
            Render(_lastRemaining, _lastTotal);
        }

        private void RefreshSuffix()
            => _suffix = LocalizationService.Resolve(LocUI.Table, LocUI.UI_HazardQte_Timer_SecondsShort, "s");
    }
}
