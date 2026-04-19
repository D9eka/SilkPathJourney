using Internal.Scripts.UI.Components;
using TMPro;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.HazardQte
{
    public sealed class HazardTimerView
    {
        private readonly FillBar _bar;
        private readonly TextMeshProUGUI _text;

        public HazardTimerView(FillBar bar, TextMeshProUGUI text)
        {
            _bar = bar;
            _text = text;
        }

        public void Render(float remaining, float total)
        {
            _bar.SetFill(total > 0f ? remaining / total : 0f);
            _text.text = $"{Mathf.Max(0f, remaining):F2}s";
        }
    }
}
