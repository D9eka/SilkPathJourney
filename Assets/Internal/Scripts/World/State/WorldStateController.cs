using System;
using System.Collections.Generic;
using Internal.Scripts.World.Camera;
using Zenject;

namespace Internal.Scripts.World.State
{
    public class WorldStateController : IFixedTickable
    {
        public Action<WorldViewMode> OnStateChange;
        
        private readonly Dictionary<WorldViewMode, WorldStateData> _viewModesData;
        private readonly ICameraRig _cameraRig;
        
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

        public WorldStateController(Dictionary<WorldViewMode, WorldStateData> viewModesData, ICameraRig cameraRig)
        {
            _viewModesData = viewModesData;
            _cameraRig = cameraRig;
        }

        public void FixedTick()
        {
            CurrentViewMode = GetViewMode(_cameraRig.Size);
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
