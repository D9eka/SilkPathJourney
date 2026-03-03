using System;
using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.Npc.Names
{
    [Serializable]
    public sealed class NameEntry
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public CultureId Culture { get; private set; }
        [field: SerializeField] public string Name { get; private set; }

        public NameEntry(string id, CultureId culture, string name)
        {
            Id = id;
            Culture = culture;
            Name = name;
        }
    }
}
