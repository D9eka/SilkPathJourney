using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.UI.Theme
{
    public class UiColorBinder : MonoBehaviour
    {
        [SerializeField] private ColorSlot _colorSlot;
        [SerializeField] private Graphic _target;
        [SerializeField] private float _transitionDuration = 0.5f;

        private UiThemeService _themeService;
        private Coroutine _transitionCoroutine;

        private void OnEnable()
        {
            if (_themeService == null)
                return;

            _themeService.PaletteChanged -= OnPaletteChanged;
            _themeService.PaletteChanged += OnPaletteChanged;
            ApplyImmediate();
        }

        private void OnDisable()
        {
            if (_themeService != null)
                _themeService.PaletteChanged -= OnPaletteChanged;

            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
                _transitionCoroutine = null;
            }
        }

        public void Initialize(UiThemeService themeService)
        {
            _themeService = themeService;
            if (isActiveAndEnabled)
            {
                _themeService.PaletteChanged -= OnPaletteChanged;
                _themeService.PaletteChanged += OnPaletteChanged;
            }
            ApplyImmediate();
        }

        private void OnPaletteChanged(UiColorPalette palette)
        {
            ApplyColor(palette.GetColor(_colorSlot));
        }

        private void ApplyImmediate()
        {
            if (_target == null || _themeService?.ActivePalette == null)
                return;

            _target.color = _themeService.GetColor(_colorSlot);
        }

        private void ApplyColor(Color targetColor)
        {
            if (_target == null)
                return;

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            if (_transitionDuration <= 0f || !gameObject.activeInHierarchy)
            {
                _target.color = targetColor;
                return;
            }

            _transitionCoroutine = StartCoroutine(TransitionColor(targetColor));
        }

        private IEnumerator TransitionColor(Color targetColor)
        {
            Color startColor = _target.color;
            float elapsed = 0f;

            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _transitionDuration);
                _target.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            _target.color = targetColor;
            _transitionCoroutine = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_target == null)
                _target = GetComponent<Graphic>();
        }
#endif
    }
}
