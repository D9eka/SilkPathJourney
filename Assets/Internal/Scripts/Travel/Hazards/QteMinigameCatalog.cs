using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards
{
    [Serializable]
    public struct QteMinigameEntry
    {
        public HazardInputType InputType;
        public GameObject Prefab;
    }

    [CreateAssetMenu(menuName = "SPJ/Travel/QTE Minigame Catalog")]
    public class QteMinigameCatalog : ScriptableObject
    {
        [SerializeField] private QteMinigameEntry[] _entries;

        public GameObject Get(HazardInputType type)
        {
            foreach (var e in _entries)
                if (e.InputType == type) return e.Prefab;
            return null;
        }
    }
}
