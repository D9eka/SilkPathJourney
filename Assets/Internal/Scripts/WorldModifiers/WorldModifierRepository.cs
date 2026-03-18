using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.Economy;
using Internal.Scripts.Save;
using R3;

namespace Internal.Scripts.WorldModifiers
{
    public enum ModifierChangeReason { Spawned, Expired, DebugForceSpawn, CascadeFrom }

    public struct ModifierChangeEntry
    {
        public string LocationId;
        public string ModifierId;
        public bool IsAdded;
        public ModifierChangeReason Reason;
        public int Day;
    }

    public sealed class WorldModifierRepository
    {
        private readonly SaveRepository _saveRepository;
        private readonly Subject<Unit> _changed = new();

        public Observable<Unit> Changed => _changed;
        public List<ModifierChangeEntry> CityDayLog { get; } = new();
        public List<ModifierChangeEntry> RoadDayLog { get; } = new();

        public WorldModifierRepository(SaveRepository saveRepository)
        {
            _saveRepository = saveRepository;
        }

        private WorldModifierSaveData State => _saveRepository.Data.WorldModifiers;

        public void ClearDayLog()
        {
            CityDayLog.Clear();
            RoadDayLog.Clear();
        }

        public List<ActiveModifierEntry> GetCityModifiers(string cityId)
        {
            return State.CityModifiers
                .Where(m => m.LocationId == cityId)
                .ToList();
        }

        public List<ActiveModifierEntry> GetRoadModifiers(string roadId)
        {
            return State.RoadModifiers
                .Where(m => m.LocationId == roadId)
                .ToList();
        }

        public int GetCityModifierCount(string cityId)
        {
            return State.CityModifiers.Count(m => m.LocationId == cityId);
        }

        public int GetRoadModifierCount(string roadId)
        {
            return State.RoadModifiers.Count(m => m.LocationId == roadId);
        }

        public bool HasCityConflict(string cityId, string conflictGroup, EconomyDatabase economyDb)
        {
            if (string.IsNullOrEmpty(conflictGroup)) return false;
            return State.CityModifiers
                .Where(m => m.LocationId == cityId)
                .Any(m =>
                {
                    var data = economyDb.GetCityModifier(m.ModifierId);
                    return data != null && data.ConflictGroup == conflictGroup;
                });
        }

        public void AddCityModifier(string cityId, string modifierId, int startDay, int duration,
            ModifierChangeReason reason = ModifierChangeReason.Spawned)
        {
            State.CityModifiers.Add(new ActiveModifierEntry
            {
                LocationId = cityId,
                ModifierId = modifierId,
                StartDay = startDay,
                Duration = duration,
                LastSeenDay = -1
            });
            CityDayLog.Add(new ModifierChangeEntry
            {
                LocationId = cityId, ModifierId = modifierId,
                IsAdded = true, Reason = reason, Day = startDay
            });
            _changed.OnNext(Unit.Default);
        }

        public void AddRoadModifier(string roadId, string modifierId, int startDay, int duration,
            ModifierChangeReason reason = ModifierChangeReason.Spawned)
        {
            State.RoadModifiers.Add(new ActiveModifierEntry
            {
                LocationId = roadId,
                ModifierId = modifierId,
                StartDay = startDay,
                Duration = duration,
                LastSeenDay = -1
            });
            RoadDayLog.Add(new ModifierChangeEntry
            {
                LocationId = roadId, ModifierId = modifierId,
                IsAdded = true, Reason = reason, Day = startDay
            });
            _changed.OnNext(Unit.Default);
        }

        public void RemoveExpired(int currentDay)
        {
            bool removed = false;

            foreach (var m in State.CityModifiers.Where(m => m.StartDay + m.Duration <= currentDay))
                CityDayLog.Add(new ModifierChangeEntry
                {
                    LocationId = m.LocationId, ModifierId = m.ModifierId,
                    IsAdded = false, Reason = ModifierChangeReason.Expired, Day = currentDay
                });

            foreach (var m in State.RoadModifiers.Where(m => m.StartDay + m.Duration <= currentDay))
                RoadDayLog.Add(new ModifierChangeEntry
                {
                    LocationId = m.LocationId, ModifierId = m.ModifierId,
                    IsAdded = false, Reason = ModifierChangeReason.Expired, Day = currentDay
                });

            removed |= State.CityModifiers.RemoveAll(m => m.StartDay + m.Duration <= currentDay) > 0;
            removed |= State.RoadModifiers.RemoveAll(m => m.StartDay + m.Duration <= currentDay) > 0;
            if (removed) _changed.OnNext(Unit.Default);
        }

        public void MarkCitySeen(string cityId, int day)
        {
            bool changed = false;
            foreach (var entry in State.CityModifiers)
            {
                if (entry.LocationId == cityId)
                {
                    entry.LastSeenDay = day;
                    changed = true;
                }
            }
            if (changed) _changed.OnNext(Unit.Default);
        }

        public void MarkRoadSeen(string roadId, int day)
        {
            bool changed = false;
            foreach (var entry in State.RoadModifiers)
            {
                if (entry.LocationId == roadId)
                {
                    entry.LastSeenDay = day;
                    changed = true;
                }
            }
            if (changed) _changed.OnNext(Unit.Default);
        }
    }
}
