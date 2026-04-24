using Internal.Scripts.Camp;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Events.Outcomes;
using Internal.Scripts.UI.Components;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Utils;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Camp
{
    public sealed class CampActionButtonFactory
    {
        private readonly CampActionService _campActionService;
        private readonly CampActionDatabase _campActionDatabase;
        private readonly EventSelector _eventSelector;
        private readonly ResourceIconCatalog _iconCatalog;

        public CampActionButtonFactory(
            CampActionService campActionService,
            CampActionDatabase campActionDatabase,
            EventSelector eventSelector,
            ResourceIconCatalog iconCatalog)
        {
            _campActionService = campActionService;
            _campActionDatabase = campActionDatabase;
            _eventSelector = eventSelector;
            _iconCatalog = iconCatalog;
        }

        public CampActionButtonState Build(CampActionType type)
        {
            var preview = _campActionService.GetPreview(type);
            var data = _campActionDatabase.GetAction(type);

            string name = data?.GetName() ?? type.ToString();
            IconLabelEntry effect = BuildEffectEntry(type, data);
            IconLabelEntry cost = BuildCostEntry(preview.Cost);
            string hint = data?.GetHint(preview.RepeatDays) ?? string.Empty;

            return new CampActionButtonState(type, name, effect, cost, hint, preview.IsAvailable);
        }

        private IconLabelEntry BuildCostEntry(float cost)
        {
            if (cost <= 0f)
                return new IconLabelEntry(null, LocalizationService.Resolve(LocUI.Table, LocUI.UI_Camp_Free));

            var icon = _iconCatalog.Get(ResourceType.Food)?.Icon;
            return new IconLabelEntry(icon, $"-{cost:F0}");
        }

        private IconLabelEntry BuildEffectEntry(CampActionType type, CampActionData data)
        {
            if (data == null) return new IconLabelEntry(null, string.Empty);
            EventOutcomeType resource = data.AffectedResource;
            var range = CalculateEventOutcomeRange(type, resource);
            if (range == null) return new IconLabelEntry(null, string.Empty);

            ResourceType? resType = resource.ToResourceType();
            if (!resType.HasValue) return new IconLabelEntry(null, string.Empty);

            var icon = _iconCatalog.Get(resType.Value)?.Icon;
            string label = range.Value.min == range.Value.max
                ? NumberFormatter.Signed(range.Value.min)
                : $"{NumberFormatter.Signed(range.Value.min)}…{NumberFormatter.Signed(range.Value.max)}";

            return new IconLabelEntry(icon, label);
        }

        private (int min, int max)? CalculateEventOutcomeRange(CampActionType actionType, EventOutcomeType resource)
        {
            var events = _eventSelector.GetEligibleEvents(
                minor: false,
                filter: evt => CampEventOutcomeScaler.ParseCampAction(evt) == actionType);

            if (events.Count == 0) return null;

            float min = float.MaxValue, max = float.MinValue;
            bool any = false;

            foreach (EventData evt in events)
            {
                var choices = _eventSelector.GetAvailableChoices(evt);
                foreach (EventChoice choice in choices)
                {
                    float sum = 0f;
                    if (choice.Outcomes != null)
                        foreach (EventOutcomeEntry o in choice.Outcomes)
                            if (o.Type == resource) sum += o.Value;
                    min = Mathf.Min(min, sum);
                    max = Mathf.Max(max, sum);
                    any = true;
                }
            }
            if (!any) return null;
            float mult = _campActionService.GetNextDiminishingMultiplier(actionType);
            int scaledMin = Mathf.RoundToInt(min * mult);
            int scaledMax = Mathf.RoundToInt(max * mult);
            return (scaledMin, scaledMax);
        }
    }
}
