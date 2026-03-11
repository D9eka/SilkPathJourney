using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    public class SliderResourceIndicator : ResourceIndicator
    {
        [SerializeField] private FillBar _fillBar;

        public void SetValue(float current, float max)
        {
            SetValue($"{current:0}");
            if (_fillBar != null)
                _fillBar.SetFill(max > 0 ? current / max : 0);
        }

        public override void ApplyAnimated(float value, float maxValue, int change,
            bool increaseIsPositive, float duration)
        {
            base.ApplyAnimated(value, maxValue, change, increaseIsPositive, duration);
            if (_fillBar != null && maxValue > 0f)
                _fillBar.AnimateFill(value / maxValue, duration);
        }

        public override void ApplyImmediate(float value, float maxValue)
        {
            SetValue(value, maxValue);
        }
    }
}
