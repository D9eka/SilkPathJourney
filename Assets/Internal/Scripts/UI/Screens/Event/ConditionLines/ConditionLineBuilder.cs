using System.Collections.Generic;

namespace Internal.Scripts.UI.Screens.Event.ConditionLines
{
    public class ConditionLineBuilder
    {
        private readonly IConditionLine[] _lines;

        public ConditionLineBuilder(
            SkillCheckConditionLine skillCheck,
            ItemConditionLine item,
            LanguageConditionLine language,
            CompanionConditionLine companion)
        {
            _lines = new IConditionLine[] { skillCheck, item, language, companion };
        }

        public string Build(ConditionLineContext context)
        {
            List<string> parts = null;

            foreach (IConditionLine line in _lines)
            {
                string text = line.GetLine(context);
                if (text == null) continue;

                parts ??= new List<string>();
                parts.Add(text);
            }

            return parts == null ? null : string.Join("\n", parts);
        }
    }
}
