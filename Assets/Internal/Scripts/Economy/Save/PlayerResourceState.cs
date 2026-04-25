using System;
using System.Collections.Generic;
using System.Linq;
using Internal.Scripts.UI.Components;

namespace Internal.Scripts.Economy.Save
{
    [Serializable]
    public class PlayerResourceState
    {
        public const string DEFAULT_CART_CLASS = "balanced";
        public const string DEFAULT_UPGRADE_LEVEL = "base";
        public const string DEFAULT_DRAFT_ANIMAL = "camel";
        public const int MAX_COMPANION_LIMIT = 3;
        public const float MORALE_MIN = 0f;
        public const float MORALE_MAX = 100f;
        public const int REPUTATION_MIN = 0;
        public const int REPUTATION_MAX = 100;

        public int Money;
        public float Food = 50f;
        public float AccumulatedDanger;
        public CartState PlayerCart = new();
        public List<CartState> Carts = new();
        public string CartClassId = DEFAULT_CART_CLASS;
        public string CartUpgradeLevelId = DEFAULT_UPGRADE_LEVEL;
        public string DraftAnimalId = DEFAULT_DRAFT_ANIMAL;
        public List<CompanionState> Companions = new();
        public List<string> ActiveUpgrades = new();
        public int Reputation = 50;
        public float Morale = 50f;
        public int SmugglingCaughtCount;
        public List<CityTradeBan> CityTradeBans = new();

        public int MaxCompanions => Math.Min(Carts?.Count ?? 0, MAX_COMPANION_LIMIT);
        public float TotalCapacity => PlayerCart.Capacity + (Carts?.Sum(c => c.Capacity) ?? 0f);

        public void ClampValues()
        {
            Money = Math.Max(0, Money);
            Morale = Math.Clamp(Morale, MORALE_MIN, MORALE_MAX);
            Reputation = Math.Clamp(Reputation, REPUTATION_MIN, REPUTATION_MAX);
        }

        public float GetValue(ResourceType type) => type switch
        {
            ResourceType.Money => Money,
            ResourceType.Food => Food,
            ResourceType.Danger => AccumulatedDanger,
            ResourceType.PlayerCartDurability => PlayerCart.Durability,
            ResourceType.OtherCartsDurability => Carts.Count > 0
                ? Carts.Sum(c => c.Durability) / Carts.Count : 0f,
            ResourceType.Weight => TotalCapacity,
            ResourceType.Reputation => Reputation,
            ResourceType.Morale => Morale,
            _ => 0f
        };
    }

    [Serializable]
    public class CityTradeBan
    {
        public string CityId;
        public int UntilDay;
    }
}
