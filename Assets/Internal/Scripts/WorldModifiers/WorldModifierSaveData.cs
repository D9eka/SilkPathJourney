using System;
using System.Collections.Generic;

namespace Internal.Scripts.WorldModifiers
{
    [Serializable]
    public class WorldModifierSaveData
    {
        public List<ActiveModifierEntry> CityModifiers = new();
        public List<ActiveModifierEntry> RoadModifiers = new();
    }

    [Serializable]
    public class ActiveModifierEntry
    {
        public string LocationId;
        public string ModifierId;
        public int StartDay;
        public int Duration;
        public int LastSeenDay = -1;
    }
}
