using System.Collections.Generic;
using Internal.Scripts.Economy.Generated;
using Internal.Scripts.UI.Theme;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Economy.Cities.UI
{
    public class CityViewSpawner : IInitializable
    {
        private readonly StaticColorController _colorController;
        private readonly List<CityView> _views = new();

        public CityViewSpawner(StaticColorController colorController)
        {
            _colorController = colorController;
        }

        public IReadOnlyList<CityView> Views => _views;

        public void Initialize()
        {
            _views.AddRange(Object.FindObjectsByType<CityView>(FindObjectsSortMode.None));

            foreach (CityView view in _views)
            {
                if (view.City != null)
                    view.ApplyBiomeColor(_colorController.GetColor(view.City.Biome, ColorSlot.CityMarker));
            }

            Debug.Log($"[SPJ] City views found: {_views.Count}");
        }

        public CityView FindByNodeId(string nodeId)
        {
            foreach (CityView view in _views)
                if (view.City != null && view.City.NodeId == nodeId)
                    return view;
            return null;
        }
    }
}
