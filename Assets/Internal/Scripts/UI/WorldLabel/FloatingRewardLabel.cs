using DG.Tweening;
using Internal.Scripts.UI.Components;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public sealed class FloatingRewardLabel : MonoBehaviour
    {
        [SerializeField] private IconLabel _content;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _riseHeight = 1.2f;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private Ease _moveEase = Ease.OutCubic;

        public void Play(Sprite icon, string text)
        {
            _content.Initialize(icon, text);
            _canvasGroup.alpha = 1f;

            var rt = (RectTransform)transform;
            Vector3 startLocal = rt.anchoredPosition3D;

            DOTween.Sequence()
                .SetLink(gameObject)
                .Join(DOTween.To(
                    () => rt.anchoredPosition3D,
                    v => rt.anchoredPosition3D = v,
                    startLocal + Vector3.up * _riseHeight,
                    _duration).SetEase(_moveEase))
                .Join(_canvasGroup.DOFade(0f, _duration).SetEase(Ease.InQuad))
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
