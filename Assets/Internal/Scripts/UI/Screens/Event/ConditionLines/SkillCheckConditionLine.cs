using System.Collections.Generic;
using Internal.Scripts.Events;
using Internal.Scripts.Events.Data;
using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Args;
using Internal.Scripts.UI.Localization.Generated;
using Internal.Scripts.UI.Screens.Trader;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.Event.ConditionLines
{
    public class SkillCheckConditionLine : IConditionLine
    {
        private readonly SkillCheckService _skillCheckService;
        private readonly TraderUICatalog _catalog;

        public SkillCheckConditionLine(SkillCheckService skillCheckService, TraderUICatalog catalog)
        {
            _skillCheckService = skillCheckService;
            _catalog = catalog;
        }

        public string GetLine(ConditionLineContext context)
        {
            SkillCheckData? sc = context.EventData.GetSkillCheck(context.OriginalChoiceIndex);
            if (!sc.HasValue) return null;

            string skillName = _catalog.GetSkillName(sc.Value.SkillType);
            int pct = Mathf.RoundToInt(
                _skillCheckService.CalculateSkillChance(sc.Value.SkillType, sc.Value.BaseChance) * 100);

            string format = LocalizationService.Resolve(LocEvents.Table, LocEvents.Event_SkillCheckInfo);

            return LocArgRenderer.Format(format, new List<ILocArg>
            {
                new TextLocArg("skill_name", skillName),
                new TextLocArg("chance", pct.ToString())
            });
        }
    }
}
