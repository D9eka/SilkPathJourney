using System;
using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.World.State
{
    [CreateAssetMenu(fileName = "WorldState", menuName = "WorldState")]
    public class WorldStatesData : ScriptableObject
    {
        [SerializeField] private List<WorldStateData> _data;
        public Dictionary<WorldViewMode, WorldStateData> ViewModesData => GetViewModesData();

        private Dictionary<WorldViewMode, WorldStateData> GetViewModesData()
        {
            Dictionary<WorldViewMode, WorldStateData> viewModes = new Dictionary<WorldViewMode, WorldStateData>();
            foreach (WorldStateData data in _data)
            {
                viewModes[data.ViewMode] = data;
            }
            return viewModes;
        }
    }
}
