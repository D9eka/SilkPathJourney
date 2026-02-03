using System.Collections.Generic;
using Internal.Scripts.Economy.Save.Models;
using UnityEngine;

namespace Internal.Scripts.Trading
{
    public sealed class TradeSession
    {
        private readonly Dictionary<string, int> _playerBase = new();
        private readonly Dictionary<string, int> _npcBase = new();
        private readonly Dictionary<string, int> _toBuy = new();
        private readonly Dictionary<string, int> _toSell = new();

        public IReadOnlyDictionary<string, int> PlayerBase => _playerBase;
        public IReadOnlyDictionary<string, int> NpcBase => _npcBase;
        public IReadOnlyDictionary<string, int> ToBuy => _toBuy;
        public IReadOnlyDictionary<string, int> ToSell => _toSell;

        public void Clear()
        {
            _playerBase.Clear();
            _npcBase.Clear();
            ClearToDictionaries();
        }

        public void ClearToDictionaries()
        {
            _toBuy.Clear();
            _toSell.Clear();
        }

        public void SetPlayerBase(InventoryState inventory)
        {
            BuildBaseDictionary(_playerBase, inventory);
        }

        public void SetNpcBase(InventoryState inventory)
        {
            BuildBaseDictionary(_npcBase, inventory);
        }

        public bool MoveToBuy(string itemId, bool addAll)
        {
            int available = GetAvailableCount(_npcBase, _toBuy, itemId);
            int count = addAll ? available : Mathf.Min(1, available);
            if (count <= 0)
                return false;

            _toBuy[itemId] = _toBuy.GetValueOrDefault(itemId) + count;
            return true;
        }

        public bool MoveToSell(string itemId, bool addAll)
        {
            int available = GetAvailableCount(_playerBase, _toSell, itemId);
            int count = addAll ? available : Mathf.Min(1, available);
            if (count <= 0)
                return false;

            _toSell[itemId] = _toSell.GetValueOrDefault(itemId) + count;
            return true;
        }

        public bool ReturnFromBuy(string itemId, bool addAll)
        {
            if (!_toBuy.TryGetValue(itemId, out int current))
                return false;

            int count = addAll ? current : 1;
            current -= count;
            if (current <= 0)
                _toBuy.Remove(itemId);
            else
                _toBuy[itemId] = current;

            return true;
        }

        public bool ReturnFromSell(string itemId, bool addAll)
        {
            if (!_toSell.TryGetValue(itemId, out int current))
                return false;

            int count = addAll ? current : 1;
            current -= count;
            if (current <= 0)
                _toSell.Remove(itemId);
            else
                _toSell[itemId] = current;

            return true;
        }

        public void ClampReserved()
        {
            ClampReserved(_playerBase, _toSell);
            ClampReserved(_npcBase, _toBuy);
        }

        private static int GetAvailableCount(Dictionary<string, int> baseCounts, Dictionary<string, int> reserved, string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                return 0;

            if (!baseCounts.TryGetValue(itemId, out int total))
                return 0;

            reserved.TryGetValue(itemId, out int used);
            return Mathf.Max(0, total - used);
        }

        private static void BuildBaseDictionary(Dictionary<string, int> target, InventoryState inventory)
        {
            target.Clear();
            if (inventory == null || inventory.Items == null)
                return;

            foreach (ItemStackState stack in inventory.Items)
            {
                if (stack == null || string.IsNullOrWhiteSpace(stack.ItemId) || stack.Count <= 0)
                    continue;

                target[stack.ItemId] = stack.Count;
            }
        }

        private static void ClampReserved(Dictionary<string, int> baseCounts, Dictionary<string, int> reserved)
        {
            List<string> toRemove = null;
            foreach (KeyValuePair<string, int> kvp in reserved)
            {
                if (!baseCounts.TryGetValue(kvp.Key, out int total) || total <= 0)
                {
                    toRemove ??= new List<string>();
                    toRemove.Add(kvp.Key);
                    continue;
                }

                if (kvp.Value > total)
                    reserved[kvp.Key] = total;
            }

            if (toRemove == null)
                return;

            foreach (string key in toRemove)
                reserved.Remove(key);
        }
    }
}
