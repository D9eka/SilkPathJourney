using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Internal.Scripts.Meta
{
    public sealed class PersistentProgressService
    {
        private const string FILE_NAME = "persistent_progress.json";

        private readonly string _filePath;
        private PersistentProgressData _data;

        public int LegacyPoints => _data.LegacyPoints;
        public IReadOnlyList<string> UnlockedIds => _data.UnlockedIds;
        public IReadOnlyList<string> EarnedAchievementIds => _data.EarnedAchievementIds;
        public LifetimeStatsData Lifetime => _data.Lifetime;

        public PersistentProgressService()
        {
            _filePath = Path.Combine(Application.persistentDataPath, FILE_NAME);
            Load();
        }

        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                _data = new PersistentProgressData();
                return;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                _data = JsonUtility.FromJson<PersistentProgressData>(json) ?? new PersistentProgressData();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to load persistent progress: {e.Message}");
                _data = new PersistentProgressData();
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(_data, true);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to save persistent progress: {e.Message}");
            }
        }

        public void AddLegacyPoints(int amount)
        {
            if (amount <= 0) return;
            _data.LegacyPoints += amount;
            Save();
        }

        public bool TrySpendLegacyPoints(int amount)
        {
            if (amount <= 0 || _data.LegacyPoints < amount)
                return false;
            _data.LegacyPoints -= amount;
            Save();
            return true;
        }

        public bool Unlock(string id)
        {
            if (string.IsNullOrEmpty(id) || _data.UnlockedIds.Contains(id))
                return false;
            _data.UnlockedIds.Add(id);
            Save();
            return true;
        }

        public bool RecordAchievement(string id)
        {
            if (string.IsNullOrEmpty(id) || _data.EarnedAchievementIds.Contains(id))
                return false;
            _data.EarnedAchievementIds.Add(id);
            Save();
            return true;
        }

        public void RecordRunCompleted(RunStatsData stats, EndType endType, int legacyEarned)
        {
            var lt = _data.Lifetime;
            lt.RunsCompleted++;

            if (endType == EndType.Victory)
                lt.RunsVictory++;
            else if (endType != EndType.None)
                lt.RunsDefeat++;

            if (stats != null)
            {
                lt.TotalDaysTravelled += stats.DaysTravelled;
                lt.TotalMoneyEarned += stats.MoneyEarned;

                if (stats.DaysTravelled > lt.BestRunDays)
                {
                    lt.BestRunDays = stats.DaysTravelled;
                    lt.BestRunEndType = stats.EndReasonKey ?? endType.ToString();
                    lt.BestRunLegacyEarned = legacyEarned;
                }
            }

            Save();
        }
    }
}
