using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using UnityEngine;

namespace Internal.Scripts.Camp
{
    [Serializable]
    public struct BiomeModifier
    {
        public Biome Biome;
        public CampActionType ActionType;
        public float Modifier;
    }

    [CreateAssetMenu(menuName = "SPJ/Camp/Camp Action Database", fileName = "CampActionDatabase")]
    public class CampActionDatabase : ScriptableObject
    {
        [field: SerializeField] public List<CampActionData> Actions { get; private set; } = new();
        [field: SerializeField] public List<BiomeModifier> BiomeModifiers { get; private set; } = new();

        public CampActionData GetAction(CampActionType type)
        {
            foreach (var action in Actions)
                if (action != null && action.Type == type)
                    return action;
            return null;
        }

        public float GetBiomeModifier(Biome biome, CampActionType type)
        {
            if (biome == Biome.Unknown)
                return 0f;

            foreach (var mod in BiomeModifiers)
                if (mod.Biome == biome && mod.ActionType == type)
                    return mod.Modifier;

            return 0f;
        }
    }
}
