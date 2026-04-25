using System.Collections.Generic;
using Internal.Scripts.Caravan;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Args;
using Internal.Scripts.UI.Localization.Generated;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Event.ConditionLines
{
    public class CompanionConditionLine : IConditionLine
    {
        private readonly CaravanDatabase _caravanDatabase;

        public CompanionConditionLine(CaravanDatabase caravanDatabase)
        {
            _caravanDatabase = caravanDatabase;
        }

        public string GetLine(ConditionLineContext context)
        {
            if (context.Choice.Conditions == null || context.Choice.Conditions.Count == 0)
                return null;

            List<string> lines = null;

            foreach (EventCondition cond in context.Choice.Conditions)
            {
                string line = BuildLine(cond);
                if (line == null) continue;

                lines ??= new List<string>();
                lines.Add(line);
            }

            return lines == null ? null : string.Join("\n", lines);
        }

        private string BuildLine(EventCondition cond)
        {
            if (cond.Type == EventConditionType.HasCompanion)
            {
                string format = LocalizationService.Resolve(LocEvents.Table, LocEvents.Event_CompanionConditionInfo_Has);
                string companionName = ResolveCompanionName(cond.Param);
                return LocArgRenderer.Format(format, new List<ILocArg>
                {
                    new TextLocArg("companion_name", companionName)
                });
            }

            if (cond.Type == EventConditionType.MinCompanions)
            {
                string format = LocalizationService.Resolve(LocEvents.Table, LocEvents.Event_CompanionConditionInfo_Min);
                int count = Mathf.RoundToInt(cond.Value);
                return LocArgRenderer.Format(format, new List<ILocArg>
                {
                    new TextLocArg("count", count.ToString())
                });
            }

            return null;
        }

        private string ResolveCompanionName(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
                return typeId ?? string.Empty;

            CompanionTypeData typeData = _caravanDatabase.GetCompanionTypeById(typeId);
            if (typeData == null)
                return typeId;

            return LocalizationService.ResolveString(typeData.Name, typeId, "Companion.Type");
        }
    }
}
