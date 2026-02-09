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

        public void AnimateValue(float current, float max, float duration = 0.5f)
        {
            SetValue($"{current:0}");
            if (_fillBar != null)
                _fillBar.AnimateFill(max > 0 ? current / max : 0, duration);
        }
    }
}
