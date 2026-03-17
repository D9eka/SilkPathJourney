using System;
using System.Collections.Generic;
using Internal.Scripts.Caravan.Generated;
using UnityEngine;

namespace Internal.Scripts.Player
{
    [CreateAssetMenu(menuName = "SPJ/Player/Config", fileName = "PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Serializable]
        public struct StartItemEntry
        {
            [field: SerializeField] public string ItemId { get; private set; }
            [field: SerializeField] public int Count { get; private set; }
        }

        [Serializable]
        public struct StartCartEntry
        {
            [field: SerializeField] public float Capacity { get; private set; }
            [field: SerializeField] public float Durability { get; private set; }
        }

        [field: SerializeField] public CartClass StartCartClass { get; private set; }
        [field: SerializeField] public string StartNodeId { get; private set; } = "N_Quanzhou";
        [field: SerializeField] public int StartMoney { get; private set; } = 200;
        [field: SerializeField] public float StartFood { get; private set; } = 50f;
        [field: SerializeField] public List<StartItemEntry> StartItems { get; private set; } = new();
        [field: SerializeField] public List<StartCartEntry> StartCarts { get; private set; } = new();
    }
}
