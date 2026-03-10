using System;
using System.Collections.Generic;
using Internal.Scripts.Player.Languages.Generated;

namespace Internal.Scripts.Player.Languages
{
    [Serializable]
    public struct LanguageEntry
    {
        public LanguageType Type;
        public LanguageProficiency Proficiency;
    }

    [Serializable]
    public class PlayerLanguageState
    {
        public List<LanguageEntry> Languages = new();

        public LanguageProficiency GetProficiency(LanguageType type)
        {
            foreach (LanguageEntry entry in Languages)
                if (entry.Type == type)
                    return entry.Proficiency;
            return LanguageProficiency.None;
        }

        public void SetProficiency(LanguageType type, LanguageProficiency proficiency)
        {
            for (int i = 0; i < Languages.Count; i++)
            {
                if (Languages[i].Type == type)
                {
                    LanguageEntry e = Languages[i];
                    e.Proficiency = proficiency;
                    Languages[i] = e;
                    return;
                }
            }

            Languages.Add(new LanguageEntry
            {
                Type = type,
                Proficiency = proficiency
            });
        }
    }
}
