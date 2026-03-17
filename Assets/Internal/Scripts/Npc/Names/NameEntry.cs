using System;
using Internal.Scripts.Economy.Generated;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.Npc.Names
{
    [Serializable]
    public sealed class NameEntry
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public CultureId Culture { get; private set; }
        [field: SerializeField] public LocalizedString Name { get; private set; }

        public NameEntry(string id, CultureId culture, LocalizedString name)
        {
            Id = id;
            Culture = culture;
            Name = name;
        }
    }
}
