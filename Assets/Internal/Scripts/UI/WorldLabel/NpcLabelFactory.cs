using System;
using Internal.Scripts.UI.Tooltip;
using Internal.Scripts.UI.WorldLabel.Components;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.WorldLabel
{
    public class NpcLabelFactory : IDisposable
    {
        private readonly WorldCanvas _worldCanvas;
        private NameLabelView _label;
        private NpcLabelView _npcLabel;

        public NameLabelView Label => _label;
        public NpcLabelView NpcLabel => _npcLabel;

        public NpcLabelFactory(WorldCanvas worldCanvas)
        {
            _worldCanvas = worldCanvas;
        }

        public NameLabelView CreateLabel(
            Vector3 worldPosition,
            string name,
            LocalizedString localizedText,
            string fallbackText,
            ITooltipDataProvider tooltipProvider = null)
        {
            return CreateLabel(worldPosition, Vector3.zero, name, localizedText, fallbackText, tooltipProvider);
        }

        public NameLabelView CreateLabel(
            Vector3 worldPosition,
            Vector3 offset,
            string name,
            LocalizedString localizedText,
            string fallbackText,
            ITooltipDataProvider tooltipProvider = null)
        {
            if (_worldCanvas == null) return null;

            var cityLabel = _worldCanvas.CreateLabel(worldPosition, offset, name);
            _label = cityLabel._nameLabel;
            _label.SetLocalizedText(localizedText, fallbackText);
            if (tooltipProvider != null)
                _label.SetTooltipProvider(tooltipProvider);

            return _label;
        }

        public NameLabelView CreateLabel(
            Vector3 worldPosition,
            string name,
            string text,
            ITooltipDataProvider tooltipProvider = null)
        {
            return CreateLabel(worldPosition, Vector3.zero, name, text, tooltipProvider);
        }

        public NameLabelView CreateLabel(
            Vector3 worldPosition,
            Vector3 offset,
            string name,
            string text,
            ITooltipDataProvider tooltipProvider = null)
        {
            if (_worldCanvas == null) return null;

            var cityLabel = _worldCanvas.CreateLabel(worldPosition, offset, name);
            _label = cityLabel._nameLabel;
            _label.SetText(text);
            if (tooltipProvider != null)
                _label.SetTooltipProvider(tooltipProvider);

            return _label;
        }

        public NpcLabelView CreateNpcLabel(
            Vector3 worldPosition,
            Vector3 offset,
            string name,
            LocalizedString localizedText,
            string fallbackText)
        {
            if (_worldCanvas == null) return null;

            _npcLabel = _worldCanvas.CreateNpcLabel(worldPosition, offset, name);
            _label = _npcLabel._nameLabel;
            _label.SetLocalizedText(localizedText, fallbackText);
            return _npcLabel;
        }

        public NpcLabelView CreateNpcLabel(
            Vector3 worldPosition,
            Vector3 offset,
            string name,
            string text)
        {
            if (_worldCanvas == null) return null;

            _npcLabel = _worldCanvas.CreateNpcLabel(worldPosition, offset, name);
            _label = _npcLabel._nameLabel;
            _label.SetText(text);
            return _npcLabel;
        }

        public void Dispose()
        {
            if (_label != null)
            {
                UnityEngine.Object.Destroy(_label.gameObject);
                _label = null;
                _npcLabel = null;
            }
        }
    }
}
