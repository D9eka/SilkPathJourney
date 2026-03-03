using System;
using System.Collections.Generic;

namespace Internal.Scripts.Npc.Save
{
    [Serializable]
    public class NpcSaveData
    {
        public List<NpcAgentSaveState> Agents = new();
    }
}
