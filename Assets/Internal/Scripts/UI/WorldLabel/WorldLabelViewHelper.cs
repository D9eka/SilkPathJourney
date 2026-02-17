using System;
using Internal.Scripts.UI.Tooltip;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.WorldLabel
{
    public class WorldLabelViewHelper : IDisposable
    {
        private readonly WorldCanvas _worldCanvas;
        private WorldLabelView _label;

        public WorldLabelView Label => _label;

        public WorldLabelViewHelper(WorldCanvas worldCanvas)
        {
            _worldCanvas = worldCanvas;
        }

        public WorldLabelView CreateLabel(
            Vector3 worldPosition,
            string name,
            LocalizedString localizedText,
            string fallbackText,
            ITooltipDataProvider tooltipProvider = null)
        {
            return CreateLabel(worldPosition, Vector3.zero, name, localizedText, fallbackText, tooltipProvider);
        }
        public WorldLabelView CreateLabel(
            Vector3 worldPosition,
            Vector3 offset,
            string name,
            LocalizedString localizedText,
            string fallbackText,
            ITooltipDataProvider tooltipProvider = null)
        {
            if (_worldCanvas == null) return null;

            _label = _worldCanvas.CreateLabel(worldPosition, offset, name);
            _label.SetLocalizedText(localizedText, fallbackText);
            if (tooltipProvider != null)
                _label.SetTooltipProvider(tooltipProvider);

            return _label;
        }

        public WorldLabelView CreateLabel(
            Vector3 worldPosition,
            string name,
            string text,
            ITooltipDataProvider tooltipProvider = null)
        {
            return CreateLabel(worldPosition, Vector3.zero, name, text, tooltipProvider);
        }
        public WorldLabelView CreateLabel(
            Vector3 worldPosition,
            Vector3 offset,
            string name,
            string text,
            ITooltipDataProvider tooltipProvider = null)
        {
            if (_worldCanvas == null) return null;

            _label = _worldCanvas.CreateLabel(worldPosition, offset, name);
            _label.SetText(text);
            if (tooltipProvider != null)
                _label.SetTooltipProvider(tooltipProvider);

            return _label;
        }

        public void Dispose()
        {
            if (_label != null)
            {
                UnityEngine.Object.Destroy(_label.gameObject);
                _label = null;
            }
        }
    }
}

