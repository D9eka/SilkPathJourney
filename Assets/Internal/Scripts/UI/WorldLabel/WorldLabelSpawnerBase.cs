using System;
using System.Collections.Generic;
using Internal.Scripts.World.State;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.UI.WorldLabel
{
    /// <summary>
    /// Base class for spawners that create WorldLabelView instances tied to WorldViewMode.
    /// Handles lifecycle, state management, and label cleanup.
    /// </summary>
    public abstract class WorldLabelSpawnerBase : IInitializable, IDisposable
    {
        protected readonly WorldStateController _worldStateController;
        protected readonly WorldCanvas _worldCanvas;
        protected readonly List<WorldLabelView> _labels = new();

        protected WorldLabelSpawnerBase(
            WorldStateController worldStateController,
            WorldCanvas worldCanvas)
        {
            _worldStateController = worldStateController;
            _worldCanvas = worldCanvas;
        }

        public void Initialize()
        {
            SpawnLabels();
            _worldStateController.OnStateChange += OnStateChange;
            OnStateChange(_worldStateController.CurrentViewMode);
        }

        public void Dispose()
        {
            _worldStateController.OnStateChange -= OnStateChange;
            foreach (WorldLabelView label in _labels)
            {
                if (label != null)
                    UnityEngine.Object.Destroy(label.gameObject);
            }
            _labels.Clear();
        }

        /// <summary>
        /// Called during Initialize to spawn all labels.
        /// Subclasses should iterate their data sources and call CreateAndConfigureLabel.
        /// </summary>
        protected abstract void SpawnLabels();

        /// <summary>
        /// Determines if labels should be visible for the given view mode.
        /// Default: visible only in RouteMap mode.
        /// </summary>
        protected virtual bool ShouldShowInViewMode(WorldViewMode viewMode)
        {
            return viewMode == WorldViewMode.RouteMap;
        }

        private void OnStateChange(WorldViewMode viewMode)
        {
            bool show = ShouldShowInViewMode(viewMode);
            foreach (WorldLabelView label in _labels)
            {
                if (show) label.Show();
                else label.Hide();
            }
        }

        /// <summary>
        /// Helper method to create label and add to managed list.
        /// </summary>
        protected WorldLabelView CreateAndConfigureLabel(Vector3 position, string name)
        {
            WorldLabelView label = _worldCanvas.CreateLabel(position, name);
            _labels.Add(label);
            return label;
        }
    }
}
