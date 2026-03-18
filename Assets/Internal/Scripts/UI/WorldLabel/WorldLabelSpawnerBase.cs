using System;
using System.Collections.Generic;
using Internal.Scripts.World.State;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.UI.WorldLabel
{
    public abstract class WorldLabelSpawnerBase : IInitializable, IDisposable
    {
        protected readonly WorldStateController _worldStateController;
        protected readonly WorldCanvas _worldCanvas;
        protected readonly List<CityLabelView> _labels = new();

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
            OnDispose();
            _worldStateController.OnStateChange -= OnStateChange;
            foreach (CityLabelView label in _labels)
            {
                if (label != null)
                    UnityEngine.Object.Destroy(label.gameObject);
            }
            _labels.Clear();
        }

        protected virtual void OnDispose() { }

        protected abstract void SpawnLabels();

        protected virtual bool ShouldShowInViewMode(WorldViewMode viewMode)
        {
            return viewMode == WorldViewMode.Strategic;
        }

        private void OnStateChange(WorldViewMode viewMode)
        {
            bool show = ShouldShowInViewMode(viewMode);
            foreach (CityLabelView label in _labels)
            {
                if (show) label._nameLabel.Show();
                else label._nameLabel.Hide();
            }
        }

        protected CityLabelView CreateAndConfigureLabel(Vector3 position, string name)
        {
            CityLabelView label = _worldCanvas.CreateLabel(position, name);
            _labels.Add(label);
            return label;
        }
    }
}
