using System.Collections.Generic;
using Internal.Scripts.Npc.Data;

namespace Internal.Scripts.Npc.Behavior
{
    public class NpcBehaviorProfileRegistry
    {
        private readonly Dictionary<NpcArchetype, NpcBehaviorProfile> _profiles = new();
        private readonly NpcBehaviorProfile _defaultProfile;

        public NpcBehaviorProfileRegistry(NpcBehaviorProfile defaultProfile, IEnumerable<NpcBehaviorProfile> profiles = null)
        {
            _defaultProfile = defaultProfile;
            if (profiles != null)
            {
                foreach (var p in profiles)
                    _profiles[p.Archetype] = p;
            }
        }

        public NpcBehaviorProfile GetProfile(NpcArchetype archetype) =>
            _profiles.TryGetValue(archetype, out var p) ? p : _defaultProfile;
    }
}
