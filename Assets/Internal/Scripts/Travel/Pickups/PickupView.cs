using System;
using DG.Tweening;
using Internal.Scripts.InteractableObjects;
using UnityEngine;

namespace Internal.Scripts.Travel.Pickups
{
    public sealed class PickupView : InteractableObjectView
    {
        public PickupData Data { get; private set; }

        private Tween _idleTween;

        public void Initialize(PickupData data)
        {
            Data = data;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            StartIdlePulse();
        }

        private void OnDisable()
        {
            _idleTween?.Kill();
            _idleTween = null;
        }

        private void StartIdlePulse()
        {
            _idleTween?.Kill();
            _idleTween = transform
                .DOScale(OriginalScale * 1.05f, 0.6f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetLink(gameObject);
        }

        public void PlayCollectAnimation(Vector3 targetWorldPosition, Action onComplete)
        {
            _idleTween?.Kill();
            _idleTween = null;

            transform.DOKill();

            Sequence seq = DOTween.Sequence().SetLink(gameObject);
            seq.Append(transform.DOScale(OriginalScale * 1.3f, 0.1f).SetEase(Ease.OutQuad));
            seq.Append(transform.DOMove(targetWorldPosition, 0.35f).SetEase(Ease.InQuad));
            seq.Join(transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InQuad));
            seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
        }

        public override void Disable()
        {
            _idleTween?.Kill();
            _idleTween = null;
            transform.DOKill();
            transform.DOScale(Vector3.zero, 0.3f)
                .SetLink(gameObject)
                .OnComplete(() => gameObject.SetActive(false));
        }

        protected override void OnClickEffect()
        {
        }
    }
}
