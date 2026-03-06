using System;
using System.Collections.Generic;

namespace Internal.Scripts.Player.Skills
{
    [Serializable]
    public struct SkillEntry
    {
        public SkillType Type;
        public int Value;
    }

    [Serializable]
    public class PlayerSkillState
    {
        public const int MinValue = 0;

        public List<SkillEntry> Skills = new();

        public int GetSkill(SkillType type)
        {
            foreach (SkillEntry entry in Skills)
                if (entry.Type == type)
                    return entry.Value;
            return MinValue;
        }

        public void AddSkill(SkillType type, int amount, int maxValue)
        {
            for (int i = 0; i < Skills.Count; i++)
            {
                if (Skills[i].Type == type)
                {
                    SkillEntry e = Skills[i];
                    e.Value = Math.Clamp(e.Value + amount, MinValue, maxValue);
                    Skills[i] = e;
                    return;
                }
            }

            Skills.Add(new SkillEntry
            {
                Type = type,
                Value = Math.Clamp(amount, MinValue, maxValue)
            });
        }
    }
}
