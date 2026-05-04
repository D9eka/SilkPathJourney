using System;

namespace Internal.Scripts.Journal
{
    [Serializable]
    public class JournalEntry
    {
        public int Day;
        public JournalEntryType Type;
        public string TitleKey;
        public string[] Args;
        public string RelatedQuestId;
    }
}
