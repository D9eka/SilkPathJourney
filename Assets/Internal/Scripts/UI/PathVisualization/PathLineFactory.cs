using UnityEngine;

namespace Internal.Scripts.UI.PathVisualization
{
    public class PathLineFactory
    {
        private readonly Material _lineMaterial;

        public PathLineFactory(Material lineMaterial)
        {
            _lineMaterial = lineMaterial;
        }

        public PathLineView CreatePathLine(string name = "PathLine")
        {
            GameObject go = new GameObject(name);
            PathLineView view = go.AddComponent<PathLineView>();
            view.Initialize(_lineMaterial);
            return view;
        }
    }
}
