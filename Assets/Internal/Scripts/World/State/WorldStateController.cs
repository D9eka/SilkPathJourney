using System;
using System.Collections.Generic;
using Internal.Scripts.Camera.Zoom;
using Zenject;

namespace Internal.Scripts.World.State
{
    public class WorldStateController : IFixedTickable
    {
        public Action<WorldViewMode> OnStateChange;
        
        private readonly Dictionary<WorldViewMode, WorldStateData> _viewModesData;
        private readonly ICameraZoomer _cameraZoomer;
        
        private WorldViewMode _currentViewMode = WorldViewMode.CityIso;

        public WorldViewMode CurrentViewMode
        {
            get => _currentViewMode;
            private set
            {
                if (value != _currentViewMode)
                {
                    _currentViewMode = value;
                    OnStateChange?.Invoke(_currentViewMode);
                }
            }
            
        }

        public WorldStateController(Dictionary<WorldViewMode, WorldStateData> viewModesData, ICameraZoomer cameraZoomer)
        {
            _viewModesData = viewModesData;
            _cameraZoomer = cameraZoomer;
        }

        public void FixedTick()
        {
            CurrentViewMode = GetViewMode(_cameraZoomer.Size);
        }

        private WorldViewMode GetViewMode(float cameraSize)
        {
            foreach (WorldStateData viewMode in _viewModesData.Values)
            {
                if (cameraSize >= viewMode.StartCameraSize && cameraSize < viewMode.EndCameraSize)
                {
                    return viewMode.ViewMode;
                }
            }

            return WorldViewMode.Artistic;
        }
    }
}
