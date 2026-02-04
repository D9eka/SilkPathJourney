using System;
using System.Collections.Generic;
using Internal.Scripts.Items;

namespace Internal.Scripts.Trading
{
    public readonly struct TradeViewState
    {
        public readonly IReadOnlyList<ItemRowData> PlayerItems;
        public readonly IReadOnlyList<ItemRowData> NpcItems;
        public readonly IReadOnlyList<ItemRowData> BuyItems;
        public readonly IReadOnlyList<ItemRowData> SellItems;
        public readonly int PlayerItemsHash;
        public readonly int NpcItemsHash;
        public readonly int BuyItemsHash;
        public readonly int SellItemsHash;
        public readonly int PlayerMoney;
        public readonly int NpcMoney;
        public readonly int BuyTotal;
        public readonly int SellTotal;
        public readonly float ProjectedWeight;
        public readonly float MaxWeight;
        public readonly bool WeightWarning;
        public readonly bool PlayerEnoughFunds;
        public readonly bool NpcEnoughFunds;
        public readonly string NpcName;

        public TradeViewState(
            IReadOnlyList<ItemRowData> playerItems, IReadOnlyList<ItemRowData> npcItems,
            IReadOnlyList<ItemRowData> buyItems, IReadOnlyList<ItemRowData> sellItems,
            int playerItemsHash, int npcItemsHash, int buyItemsHash, int sellItemsHash,
            int playerMoney, int npcMoney, int buyTotal, int sellTotal, float projectedWeight,
            float maxWeight, bool weightWarning, bool playerEnoughFunds, bool npcEnoughFunds, string npcName)
        {
            PlayerItems = playerItems;
            NpcItems = npcItems;
            BuyItems = buyItems;
            SellItems = sellItems;
            PlayerItemsHash = playerItemsHash;
            NpcItemsHash = npcItemsHash;
            BuyItemsHash = buyItemsHash;
            SellItemsHash = sellItemsHash;
            PlayerMoney = playerMoney;
            NpcMoney = npcMoney;
            BuyTotal = buyTotal;
            SellTotal = sellTotal;
            ProjectedWeight = projectedWeight;
            MaxWeight = maxWeight;
            WeightWarning = weightWarning;
            PlayerEnoughFunds = playerEnoughFunds;
            NpcEnoughFunds = npcEnoughFunds;
            NpcName = npcName;
        }
        
        public TradeViewState(
            int playerItemsHash, int npcItemsHash, int buyItemsHash, int sellItemsHash,
            int playerMoney, int npcMoney, int buyTotal, int sellTotal, float projectedWeight,
            float maxWeight, bool weightWarning, bool playerEnoughFunds, bool npcEnoughFunds, string npcName)
        : this(Array.Empty<ItemRowData>(),Array.Empty<ItemRowData>(),Array.Empty<ItemRowData>(),
            Array.Empty<ItemRowData>(), playerItemsHash, npcItemsHash, buyItemsHash, sellItemsHash, playerMoney, 
            npcMoney, buyTotal, sellTotal, projectedWeight, maxWeight, weightWarning, playerEnoughFunds, npcEnoughFunds, npcName)
        {
        }

        public TradeViewState(bool weightWarning, bool playerEnoughFunds, bool npcEnoughFunds, string npcName)
        : this(0, 0, 0, 0, 0, 0, 0, 0,
            0f, 0f, weightWarning, playerEnoughFunds, npcEnoughFunds, npcName)
        {
        }
    }
}
