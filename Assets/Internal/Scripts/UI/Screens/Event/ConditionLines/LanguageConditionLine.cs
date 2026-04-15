using System;
using System.Collections.Generic;
using Internal.Scripts.Events.Data;
using Internal.Scripts.Events.Generated;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Args;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Trader;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Event.ConditionLines
{
    public class LanguageConditionLine : IConditionLine
    {
        private readonly TraderUICatalog _catalog;

        public LanguageConditionLine(TraderUICatalog catalog)
        {
            _catalog = catalog;
        }

        public string GetLine(ConditionLineContext context)
        {
            if (context.Choice.Conditions == null || context.Choice.Conditions.Count == 0)
                return null;

            string format = null;
            List<string> lines = null;

            foreach (EventCondition cond in context.Choice.Conditions)
            {
                if (cond.Type != EventConditionType.MinLanguageProficiency) continue;

                if (!Enum.TryParse(cond.Param, true, out LanguageType languageType) || languageType == LanguageType.None)
                    continue;

                format ??= LocalizationService.Resolve(LocEvents.Table, LocEvents.Event_LanguageConditionInfo);

                string languageName = _catalog.GetLanguageName(languageType);
                string proficiencyName = ((LanguageProficiency)Mathf.RoundToInt(cond.Value)).ToString();

                lines ??= new List<string>();
                lines.Add(LocArgRenderer.Format(format, new List<ILocArg>
                {
                    new TextLocArg("language_name", languageName),
                    new TextLocArg("proficiency", proficiencyName)
                }));
            }

            return lines == null ? null : string.Join("\n", lines);
        }
    }
}
