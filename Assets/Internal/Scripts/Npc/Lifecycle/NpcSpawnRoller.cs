using System;
using Internal.Scripts.Npc.Data;

namespace Internal.Scripts.Npc.Lifecycle
{
    public sealed class NpcSpawnRoller
    {
        private readonly NpcSimulationSettings _settings;

        public NpcSpawnRoller(NpcSimulationSettings settings)
        {
            _settings = settings;
        }

        public NpcArchetype ChooseArchetypeToSpawn(Func<NpcArchetype, int> countByArchetype)
        {
            if (_settings.Archetypes == null || _settings.Archetypes.Count == 0)
                return NpcArchetype.BazaarTrader;

            NpcArchetype best = NpcArchetype.BazaarTrader;
            int maxDeficit = int.MinValue;
            foreach (NpcArchetypeDefinition def in _settings.Archetypes)
            {
                int deficit = def.SpawnCount - countByArchetype(def.Archetype);
                if (deficit > maxDeficit) { maxDeficit = deficit; best = def.Archetype; }
            }
            return best;
        }

        public NpcExperienceLevel RollExperience(Func<int, int, int> randomRange)
        {
            int total = _settings.NoviceCount + _settings.ExperiencedCount + _settings.MasterCount;
            if (total <= 0) return NpcExperienceLevel.Novice;
            int roll = randomRange(0, total);
            if (roll < _settings.NoviceCount) return NpcExperienceLevel.Novice;
            if (roll < _settings.NoviceCount + _settings.ExperiencedCount) return NpcExperienceLevel.Experienced;
            return NpcExperienceLevel.Master;
        }

        public NpcArchetypeDefinition FindArchetypeDef(NpcArchetype archetype)
        {
            if (_settings.Archetypes == null)
                return default;
            foreach (NpcArchetypeDefinition def in _settings.Archetypes)
            {
                if (def.Archetype == archetype)
                    return def;
            }
            return _settings.Archetypes.Count > 0 ? _settings.Archetypes[0] : default;
        }
    }
}
