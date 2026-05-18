using Internal.Scripts.Economy.Generated;
using Internal.Scripts.Road.Core;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.Tooltip;
using Internal.Scripts.UI.WorldLabel.Components;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public class RoadLabelView : MonoBehaviour
    {
        [SerializeField] private ModifierIconsView _modifiers;

        private StaticColorController _colorController;
        private Biome _biome = Biome.Plains;

        private RoadRuntime _road;

        public ModifierIconsView Modifiers => _modifiers;

        public void SetColorController(StaticColorController controller, Biome biome)
        {
            _colorController = controller;
            _biome = biome;
            gameObject.InitializeColorBinders(colorController: controller, biome: biome);
        }

        public void Initialize(TooltipService tooltipService)
        {
            if (_modifiers != null)
                _modifiers.Initialize(tooltipService);
        }

        public void SetRoad(RoadRuntime road)
        {
            _road = road;
        }

        public void SetHasModifiers(bool has)
        {
            if (_modifiers != null)
                _modifiers.SetVisible(has);
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
