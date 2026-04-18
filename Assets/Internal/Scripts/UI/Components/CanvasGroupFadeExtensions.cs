using DG.Tweening;
using UnityEngine;

namespace Internal.Scripts.UI.Components
{
    public static class CanvasGroupFadeExtensions
    {
        public static Tween FadeIn(this CanvasGroup group, float duration, Tween previous = null)
        {
            previous?.Kill();
            group.gameObject.SetActive(true);
            group.alpha = 0f;
            return group.DOFade(1f, duration).SetUpdate(true);
        }

        public static Tween FadeOut(this CanvasGroup group, float duration, Tween previous = null, bool deactivateOnComplete = true)
        {
            previous?.Kill();
            Tween fade = group.DOFade(0f, duration).SetUpdate(true);
            if (deactivateOnComplete)
                fade.OnComplete(() => group.gameObject.SetActive(false));
            return fade;
        }
    }
}
