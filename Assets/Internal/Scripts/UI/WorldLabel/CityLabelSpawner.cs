using System.Collections.Generic;
using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Cities;
using Internal.Scripts.Economy.Cities.UI;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.World.State;
using UnityEngine;

namespace Internal.Scripts.UI.WorldLabel
{
    public class CityLabelSpawner : WorldLabelSpawnerBase
    {
        private const int MODIFIERS_PER_CITY = 2;

        private readonly CityViewSpawner _cityViewSpawner;
        private readonly EconomyDatabase _economyDatabase;
        private readonly StaticColorController _colorController;

        public CityLabelSpawner(
            CityViewSpawner cityViewSpawner,
            WorldStateController worldStateController,
            WorldCanvas worldCanvas,
            EconomyDatabase economyDatabase,
            StaticColorController colorController)
            : base(worldStateController, worldCanvas)
        {
            _cityViewSpawner = cityViewSpawner;
            _economyDatabase = economyDatabase;
            _colorController = colorController;
        }

        protected override bool ShouldShowInViewMode(WorldViewMode viewMode) => true;

        protected override void SpawnLabels()
        {
            int total = 0;

            foreach (CityView view in _cityViewSpawner.Views)
            {
                if (view.City == null) continue;
                total++;

                CityData city = view.City;
                Transform nodeTransform = view.transform.parent;
                Vector3 position = nodeTransform != null ? nodeTransform.position : view.transform.position;

                WorldLabelView label = CreateAndConfigureLabel(
                    position,
                    $"CityLabel_{city.Id}");
                label.SetColorController(_colorController, city.Biome);
                label.SetLocalizedText(city.Name, city.Id);
                label.SetTooltipProvider(city);

                CityTypeData cityType = _economyDatabase.GetCityType(city.Type);
                if (cityType != null)
                {
                    if (cityType.Icon != null)
                        label.SetIcon(cityType.Icon);
                    label.SetIconTooltip(cityType.GetTooltipTitle(), cityType.GetTooltipDescription());
                }

                AddRandomModifiers(label);
            }

            Debug.Log($"[CityLabelSpawner] City labels created: {total}");
        }

        private void AddRandomModifiers(WorldLabelView label)
        {
            List<CityModifierData> all = _economyDatabase.CityModifiers;
            if (all == null || all.Count == 0) return;

            int count = Mathf.Min(MODIFIERS_PER_CITY, all.Count);
            List<int> indices = new(all.Count);
            for (int i = 0; i < all.Count; i++)
                indices.Add(i);

            for (int i = 0; i < count; i++)
            {
                int pick = Random.Range(i, indices.Count);
                (indices[i], indices[pick]) = (indices[pick], indices[i]);

                CityModifierData mod = all[indices[i]];
                if (mod == null) continue;

                label.AddIcon(
                    mod.Icon,
                    mod.GetTooltipTitle(),
                    mod.GetTooltipDescription());
            }
        }
    }
}
