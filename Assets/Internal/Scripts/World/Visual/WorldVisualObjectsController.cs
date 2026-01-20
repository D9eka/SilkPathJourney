using System;
using System.Collections.Generic;
using Internal.Scripts.World.VisualObjects;
using Zenject;
using Internal.Scripts.World.State;

namespace Internal.Scripts.World.Visual
{
    public class WorldVisualObjectsController : IInitializable, IDisposable
    {
        private readonly WorldStateController _worldStateController;
        private readonly List<IVisualObject> _visualObjects;

        public WorldVisualObjectsController(WorldStateController worldStateController, List<IVisualObject> visualObjects)
        {
            _worldStateController = worldStateController;
            _visualObjects = visualObjects;
        }

        public void Initialize()
        {
            _worldStateController.OnStateChange += OnStateChange;
            OnStateChange(_worldStateController.CurrentViewMode);
        }

        public void Dispose()
        {
            _worldStateController.OnStateChange -= OnStateChange;
        }
        
        private void OnStateChange(WorldViewMode viewMode)
        {
            WorldDetailLevel worldDetailLevel = GetWorldDetailLevel(viewMode);
            foreach (IVisualObject visualObject in _visualObjects)
            {
                if (visualObject.ViewMode == worldDetailLevel || visualObject.ViewMode == WorldDetailLevel.Both)
                {
                    visualObject.Show();
                }
                else
                {
                    visualObject.Hide();
                }
            }
        }

        private WorldDetailLevel GetWorldDetailLevel(WorldViewMode viewMode)
        {
            return viewMode switch
            {
                WorldViewMode.CityIso => WorldDetailLevel.Detailed,
                WorldViewMode.RouteMap => WorldDetailLevel.Simplified,
                WorldViewMode.Artistic => WorldDetailLevel.Simplified,
                _ => throw new NotImplementedException()
            };
        }
    }
}
