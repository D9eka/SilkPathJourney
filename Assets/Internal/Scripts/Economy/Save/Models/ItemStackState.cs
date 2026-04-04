using System;
using Internal.Scripts.Economy.Generated;

namespace Internal.Scripts.Economy.Save.Models
{
    [Serializable]
    public class ItemStackState
    {
        public string ItemId;
        public int Count;
        public CultureId OriginCulture;
    }
}
