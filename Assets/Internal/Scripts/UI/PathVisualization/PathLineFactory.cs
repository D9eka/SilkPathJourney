using Internal.Scripts.UI.Theme;
using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.UI.PathVisualization
{
    public class PathLineFactory
    {
        private readonly Material _lineMaterial;
        private readonly Color _lineColor;

        public PathLineFactory(Material lineMaterial, StaticColorController colorController)
        {
            _lineMaterial = lineMaterial;
            _lineColor = colorController != null
                ? colorController.GetColor(Biome.Plains, ColorSlot.RoadHighlight)
                : Color.yellow;
        }

        public PathLineView CreatePathLine(string name = "PathLine")
        {
            GameObject go = new GameObject(name);
            PathLineView view = go.AddComponent<PathLineView>();
            view.Initialize(_lineMaterial, _lineColor);
            return view;
        }
    }
}
