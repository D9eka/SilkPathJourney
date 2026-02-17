using System;
using System.Collections.Generic;

namespace Internal.Scripts.Road.State
{
    public class RoadUnlockService
    {
        private readonly HashSet<string> _unlockedRoadIds = new();

        public event Action<string> OnRoadUnlocked;

        public bool IsUnlocked(string roadId) => _unlockedRoadIds.Contains(roadId);

        public void UnlockRoad(string roadId)
        {
            if (_unlockedRoadIds.Add(roadId))
                OnRoadUnlocked?.Invoke(roadId);
        }

        public void LoadState(List<string> unlockedIds)
        {
            _unlockedRoadIds.Clear();
            if (unlockedIds != null)
                foreach (string id in unlockedIds)
                    _unlockedRoadIds.Add(id);
        }

        public List<string> SaveState() => new(_unlockedRoadIds);
    }
}
