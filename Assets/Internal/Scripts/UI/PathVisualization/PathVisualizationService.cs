using System;
using Internal.Scripts.Road.Path;
using UnityEngine;

namespace Internal.Scripts.UI.PathVisualization
{
    public class PathVisualizationService : IPathVisualizationService, IDisposable
    {
        private readonly PathLineFactory _lineFactory;
        private readonly PathLineRenderer _lineRenderer;

        private PathLineView _currentLine;

        public PathVisualizationService(
            PathLineFactory lineFactory,
            PathLineRenderer lineRenderer)
        {
            _lineFactory = lineFactory;
            _lineRenderer = lineRenderer;
        }

        public void ShowPath(RoadPath path)
        {
            if (path == null || !path.IsValid)
            {
                HidePath();
                return;
            }

            if (_currentLine == null)
            {
                _currentLine = _lineFactory.CreatePathLine();
            }

            Vector3[] positions = _lineRenderer.RenderPath(path);
            _currentLine.SetPositions(positions);
            _currentLine.Show();
        }

        public void HidePath()
        {
            if (_currentLine != null)
            {
                _currentLine.Hide();
            }
        }

        public void UpdatePath(RoadPath path)
        {
            ShowPath(path);
        }

        public void Dispose()
        {
            if (_currentLine != null)
            {
                UnityEngine.Object.Destroy(_currentLine.gameObject);
                _currentLine = null;
            }
        }
    }
}
