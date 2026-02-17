using DG.Tweening;
using UnityEngine;

namespace Internal.Scripts.InteractableObjects
{
    public class InteractableOutline
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private readonly Renderer[] _renderers;
        private readonly Color _highlightColor;
        private readonly MaterialPropertyBlock _propertyBlock;
        private Tween _tween;

        public InteractableOutline(Renderer[] renderers, Color highlightColor)
        {
            _renderers = renderers;
            _highlightColor = highlightColor;
            _propertyBlock = new MaterialPropertyBlock();

            foreach (Renderer r in _renderers)
            {
                if (r == null) continue;
                foreach (Material mat in r.materials)
                    mat.EnableKeyword("_EMISSION");
            }
        }

        public void Show()
        {
            _tween?.Kill();
            _tween = DOVirtual.Color(Color.black, _highlightColor, 0.15f, SetEmission);
        }

        public void Hide()
        {
            _tween?.Kill();
            _tween = DOVirtual.Color(_highlightColor, Color.black, 0.15f, SetEmission);
        }

        public void Dispose() => _tween?.Kill();

        private void SetEmission(Color color)
        {
            _propertyBlock.SetColor(EmissionColorId, color);
            foreach (Renderer r in _renderers)
                if (r != null) r.SetPropertyBlock(_propertyBlock);
        }
    }
}
