using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Minigames;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public sealed class SandstormMinigame : MinigameBase
    {
        [SerializeField] private RectTransform _cart;
        [SerializeField] private Image _dangerZone;
        [SerializeField] private LayoutElement _dangerLayoutElement;
        [SerializeField] private RectTransform _safeZone;
        [SerializeField] private HorizontalLayoutGroup _roadLayout;
        [SerializeField] private TextMeshProUGUI _windDirectionLabel;

        public override bool DidPlayerSucceed() => IsAlive || Succeeded;

        private IQteInput _input;
        private float _windSpeed;
        private float _pushAmount;
        private int _windDirection;

        public override void Show(IMinigameConfig config, IQteInput input)
        {
            _input = input;
            SetAlive(true);

            var c = config as SandstormMinigameConfig;
            if (c == null) { Debug.LogError($"[SandstormMinigame] bad config: {config?.GetType().Name}"); Complete(false); return; }
            _windSpeed = c.CartSpeed;
            _pushAmount = c.ClickPush;

            bool windFromLeft = UnityEngine.Random.value < 0.5f;
            _windDirection = windFromLeft ? -1 : +1;

            _roadLayout.reverseArrangement = !windFromLeft;

            float scaleX = windFromLeft ? 1f : -1f;
            _safeZone.localScale = new Vector3(scaleX, 1f, 1f);
            _dangerZone.rectTransform.localScale = new Vector3(scaleX, 1f, 1f);

            Canvas.ForceUpdateCanvases();

            var roadRect = (RectTransform)_roadLayout.transform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(roadRect);
            float roadWidth = roadRect.rect.width;

            float ratio = UnityEngine.Random.Range(c.DangerWidthRatioMin, c.DangerWidthRatioMax);
            _dangerLayoutElement.preferredWidth = roadWidth * ratio;

            LayoutRebuilder.ForceRebuildLayoutImmediate(roadRect);

            _cart.anchoredPosition = ResolveCartStart(windFromLeft);

            UpdateWindLabel();
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

            _input.Enable();
            _input.OnClick += OnClick;
        }

        public override void Hide()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
            if (_input == null) return;
            _input.OnClick -= OnClick;
            _input.Disable();
            _input = null;
        }

        private void Update()
        {
            if (!IsAlive) return;

            var roadRect = (RectTransform)_roadLayout.transform;
            float roadWidth = roadRect.rect.width;

            var pos = _cart.anchoredPosition;
            pos.x += _windDirection * _windSpeed * Time.unscaledDeltaTime;
            if (roadWidth > 0f)
            {
                float halfCart = _cart.rect.width * 0.5f;
                pos.x = Mathf.Clamp(pos.x, halfCart, roadWidth - halfCart);
            }
            _cart.anchoredPosition = pos;

            if (UiRectOverlap.Check(_cart, _dangerZone.rectTransform))
                Complete(false);
        }

        private void OnClick()
        {
            if (!IsAlive) return;
            _cart.anchoredPosition += new Vector2(-_windDirection * _pushAmount, 0f);
        }

        private Vector2 ResolveCartStart(bool windFromLeft)
        {
            var roadRect = (RectTransform)_roadLayout.transform;
            float startX = windFromLeft ? roadRect.rect.width - 32f : 32f;
            return new Vector2(startX, _cart.anchoredPosition.y);
        }

        private void OnLocaleChanged(Locale _) => UpdateWindLabel();

        private void UpdateWindLabel()
        {
            string key = _windDirection < 0
                ? LocUI.UI_HazardQte_Wind_FromLeft
                : LocUI.UI_HazardQte_Wind_FromRight;
            string fallback = _windDirection < 0 ? "Wind \u2190" : "Wind \u2192";
            _windDirectionLabel.text = LocalizationService.Resolve(LocUI.Table, key, fallback);
        }
    }
}
