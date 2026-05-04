using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Theme;
using Internal.Scripts.UI.Utils;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.HazardQte
{
    public sealed class HazardOutcomesView
    {
        private readonly Transform _container;
        private readonly IconLabel _prefab;
        private readonly List<IconLabel> _spawned = new();

        public HazardOutcomesView(Transform container, IconLabel prefab)
        {
            _container = container;
            _prefab = prefab;
        }

        public void Set(List<EventOutcomeEntry> outcomes, ResourceIconCatalog icons, UiThemeService theme)
        {
            Clear();
            if (outcomes == null) return;
            foreach (var entry in outcomes)
            {
                if (entry.Value == 0f) continue;
                Sprite icon = ResolveIcon(entry, icons);
                string label = NumberFormatter.Signed(Mathf.RoundToInt(entry.Value));
                var view = Object.Instantiate(_prefab, _container);
                view.gameObject.InitializeColorBinders(themeService: theme);
                view.Initialize(icon, label);
                _spawned.Add(view);
            }
        }

        public void Clear()
        {
            foreach (var s in _spawned)
                if (s != null) Object.Destroy(s.gameObject);
            _spawned.Clear();
        }

        private static Sprite ResolveIcon(EventOutcomeEntry entry, ResourceIconCatalog icons)
        {
            var resourceType = entry.Type.ToResourceType();
            if (!resourceType.HasValue || icons == null) return null;
            return icons.Get(resourceType.Value)?.Icon;
        }
    }
}
