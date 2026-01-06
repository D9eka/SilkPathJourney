using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.World.VisualObjects;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.World
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
                if (visualObject.ViewMode.Any(detailLevel => detailLevel == worldDetailLevel))
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
                WorldViewMode.CityIso => WorldDetailLevel.Full,
                WorldViewMode.RouteMap => WorldDetailLevel.Simplified,
                WorldViewMode.Artistic => WorldDetailLevel.Symbolic,
                _ => throw new NotImplementedException()
            };
        }
    }
}