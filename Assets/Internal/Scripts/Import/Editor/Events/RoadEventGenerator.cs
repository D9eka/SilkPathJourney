using System.Collections.Generic;
using System.IO;
using System.Text;
using Internal.Scripts.Import.Editor.Core;
using Internal.Scripts.World.Roads;
using UnityEditor;
using UnityEngine;
using static Internal.Scripts.Import.Editor.Core.ImportHelpers;

namespace Internal.Scripts.Import.Editor.Events
{
    public static class RoadEventGenerator
    {
        private const string ROAD_EVENTS_CSV = "road_events.csv";
        private const string ROAD_CHOICES_CSV = "road_event_choices.csv";
        private const string ROAD_CONDITIONS_CSV = "road_event_conditions.csv";
        private const string ROAD_OUTCOMES_CSV = "road_event_outcomes.csv";
        private const string ROAD_LOC_CSV = "road_events_localization.csv";

        [MenuItem("SPJ/Import/Events/Generate Road Events")]
        public static void Generate()
        {
            List<RoadData> hiddenRoads = FindHiddenRoads();
            if (hiddenRoads.Count == 0)
            {
                Debug.Log("[SPJ] No hidden roads found. Skipping road event generation.");
                return;
            }

            var events = new StringBuilder();
            var choices = new StringBuilder();
            var conditions = new StringBuilder();
            var outcomes = new StringBuilder();
            var loc = new StringBuilder();

            events.AppendLine("event_id,event_type_id,name_key,description_key,image_name,image_prompt,weight,is_minor");
            choices.AppendLine("event_id,choice_index,name_key,result_key");
            conditions.AppendLine("event_id,choice_index,type,param,value");
            outcomes.AppendLine("event_id,choice_index,type,param,value");
            loc.AppendLine("key,source_sheet,source_row,ru,en");

            for (int i = 0; i < hiddenRoads.Count; i++)
            {
                RoadData road = hiddenRoads[i];
                string roadId = road.RoadId;
                string nodeId = road.StartNodeId;

                WriteDiscoveryEvent(i, roadId, nodeId, events, choices, conditions, outcomes, loc);
                WriteEncounterEvent(i, roadId, nodeId, events, choices, conditions, outcomes, loc);
                WriteQuestEvent(i, roadId, nodeId, events, choices, conditions, outcomes, loc);
            }

            WriteCsv(ROAD_EVENTS_CSV, events);
            WriteCsv(ROAD_CHOICES_CSV, choices);
            WriteCsv(ROAD_CONDITIONS_CSV, conditions);
            WriteCsv(ROAD_OUTCOMES_CSV, outcomes);
            WriteCsv(ROAD_LOC_CSV, loc);

            Debug.Log($"[SPJ] Generated road events for {hiddenRoads.Count} hidden road(s).");
        }

        private static List<RoadData> FindHiddenRoads()
        {
            List<RoadData> result = new();
            string[] guids = AssetDatabase.FindAssets("t:RoadData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RoadData data = AssetDatabase.LoadAssetAtPath<RoadData>(path);
                if (data != null && data.IsHidden && !string.IsNullOrEmpty(data.RoadId))
                    result.Add(data);
            }

            result.Sort((a, b) => string.Compare(a.RoadId, b.RoadId, System.StringComparison.Ordinal));
            return result;
        }

        #region Discovery

        private static readonly (string nameRu, string nameEn, string descRu, string descEn,
            string accept, string acceptEn, string acceptResultRu, string acceptResultEn,
            string decline, string declineEn, string declineResultRu, string declineResultEn)[] DiscoveryTemplates =
        {
            (
                "Заросшая тропа", "Overgrown Trail",
                "Сквозь густые заросли проглядывает старая, почти забытая дорога. Выбоины от колёс давно заросли травой, но направление ещё угадывается. Местные обходят это место стороной — говорят, путь опасен.",
                "An old, nearly forgotten road peers through dense undergrowth. Wheel ruts have long been swallowed by grass, but the direction is still discernible. Locals avoid this place — they say the path is treacherous.",
                "Расчистить путь", "Clear the path",
                "Караван прокладывает дорогу сквозь заросли. За поворотом открывается пригодный для движения тракт — новый маршрут найден!",
                "The caravan cuts through the overgrowth. Around the bend, a passable road opens up — a new route discovered!",
                "Не рисковать", "Don't risk it",
                "Вы решаете не сходить с проверенного пути. Заросшая тропа остаётся позади.",
                "You decide to stick to the known road. The overgrown trail fades behind you."
            ),
            (
                "Древний указатель", "Ancient Waystone",
                "Полузасыпанный камень с полустёртыми надписями указывает в сторону от основной дороги. Рядом — следы старого мощения, уходящие за холм. Кто-то когда-то проложил здесь путь.",
                "A half-buried stone with weathered inscriptions points away from the main road. Nearby, remnants of old paving stones trail off beyond a hill. Someone once built a road here.",
                "Следовать указателю", "Follow the marker",
                "За холмом открывается забытая дорога, ведущая к далёкому горизонту. Путь свободен — караван может пройти!",
                "Beyond the hill, a forgotten road stretches toward the distant horizon. The way is clear — the caravan can pass!",
                "Пройти мимо", "Pass by",
                "Камень остаётся у дороги, храня свою тайну.",
                "The stone remains by the road, keeping its secret."
            ),
            (
                "Скрытый перевал", "Hidden Pass",
                "Между скал виднеется узкий проход, замаскированный кустарником и осыпью. Птицы свободно пролетают насквозь — значит, с другой стороны есть выход. Может, это короткий путь?",
                "A narrow gap between the rocks, masked by bushes and scree, catches your eye. Birds fly freely through it — there must be an exit on the other side. Could this be a shortcut?",
                "Исследовать проход", "Explore the passage",
                "Проход оказывается достаточно широким для повозки. С другой стороны — дорога, о которой не знают караванщики!",
                "The passage is wide enough for a wagon. On the other side — a road unknown to any caravan master!",
                "Слишком рискованно", "Too risky",
                "Вы оставляете проход нетронутым и продолжаете путь.",
                "You leave the passage untouched and continue on your way."
            ),
        };

        private static void WriteDiscoveryEvent(int roadIndex, string roadId, string nodeId,
            StringBuilder events, StringBuilder choices, StringBuilder conditions,
            StringBuilder outcomes, StringBuilder loc)
        {
            var t = DiscoveryTemplates[roadIndex % DiscoveryTemplates.Length];
            string eventId = $"road_discover_{roadId}";

            events.AppendLine($"{eventId},discovery,event.{eventId}.name,event.{eventId}.description,,,2,0");

            choices.AppendLine($"{eventId},1,event.{eventId}.choice1,event.{eventId}.choice1.result");
            choices.AppendLine($"{eventId},2,event.{eventId}.choice2,event.{eventId}.choice2.result");

            conditions.AppendLine($"{eventId},,near_node,{nodeId},0");

            outcomes.AppendLine($"{eventId},1,unlock_road,{roadId},0");

            WriteLoc(loc, $"event.{eventId}.name", t.nameRu, t.nameEn);
            WriteLoc(loc, $"event.{eventId}.description", t.descRu, t.descEn);
            WriteLoc(loc, $"event.{eventId}.choice1", t.accept, t.acceptEn);
            WriteLoc(loc, $"event.{eventId}.choice1.result", t.acceptResultRu, t.acceptResultEn);
            WriteLoc(loc, $"event.{eventId}.choice2", t.decline, t.declineEn);
            WriteLoc(loc, $"event.{eventId}.choice2.result", t.declineResultRu, t.declineResultEn);
        }

        #endregion

        #region Encounter

        private static readonly (string nameRu, string nameEn, string descRu, string descEn,
            string accept, string acceptEn, string acceptResultRu, string acceptResultEn,
            string decline, string declineEn, string declineResultRu, string declineResultEn)[] EncounterTemplates =
        {
            (
                "Старый проводник", "The Old Guide",
                "Седой старик в пыльном халате сидит у обочины, чертя палкой на земле. Увидев караван, он поднимает голову и говорит, что знает дорогу, которую не показывают на картах. За небольшую плату он готов указать путь.",
                "A grey-haired old man in a dusty robe sits by the roadside, drawing in the dirt with a stick. Seeing the caravan, he looks up and says he knows a road not shown on any map. For a small fee, he'll point the way.",
                "Заплатить и выслушать", "Pay and listen",
                "Старик рисует на песке подробную карту. Его глаза блестят — он явно рад, что кто-то наконец воспользуется его знанием. Новый путь открыт!",
                "The old man draws a detailed map in the sand. His eyes gleam — he is clearly glad someone will finally use his knowledge. A new route is revealed!",
                "Отказаться", "Refuse",
                "Старик пожимает плечами и снова склоняется к своим рисункам на земле.",
                "The old man shrugs and bends back over his drawings in the dirt."
            ),
            (
                "Купец с секретом", "Merchant with a Secret",
                "Встречный торговец останавливает караван и шёпотом предлагает сделку. Он знает обходной путь, который сэкономит дни пути — но просит взамен несколько монет и обещание молчать.",
                "A passing merchant stops the caravan and whispers an offer. He knows a bypass that will save days of travel — but he wants a few coins and a promise of silence in return.",
                "Согласиться", "Agree",
                "Торговец достаёт потрёпанную карту и показывает скрытый маршрут. «Только никому», — подмигивает он. Путь открыт!",
                "The merchant produces a worn map and reveals the hidden route. 'Tell no one,' he winks. The path is open!",
                "Отвергнуть", "Decline",
                "Торговец прячет карту и уходит, бормоча что-то себе под нос.",
                "The merchant hides his map and walks off, muttering under his breath."
            ),
            (
                "Благодарный путник", "Grateful Traveler",
                "У дороги лежит раненый путник — его ограбили разбойники. Он умоляет о помощи и обещает щедро отблагодарить, если караван поделится припасами и перевяжет раны.",
                "A wounded traveler lies by the road — bandits robbed him. He begs for help and promises generous repayment if the caravan shares supplies and binds his wounds.",
                "Помочь путнику", "Help the traveler",
                "Оправившись, путник благодарит караван и раскрывает тайную тропу, о которой знают лишь местные контрабандисты. Новый маршрут открыт!",
                "Once recovered, the traveler thanks the caravan and reveals a secret trail known only to local smugglers. A new route unlocked!",
                "Пройти мимо", "Walk past",
                "Путник провожает караван горьким взглядом. Его тайна остаётся при нём.",
                "The traveler watches the caravan leave with a bitter gaze. His secret stays with him."
            ),
        };

        private static void WriteEncounterEvent(int roadIndex, string roadId, string nodeId,
            StringBuilder events, StringBuilder choices, StringBuilder conditions,
            StringBuilder outcomes, StringBuilder loc)
        {
            var t = EncounterTemplates[roadIndex % EncounterTemplates.Length];
            string eventId = $"road_encounter_{roadId}";

            events.AppendLine($"{eventId},encounter,event.{eventId}.name,event.{eventId}.description,,,1,0");

            choices.AppendLine($"{eventId},1,event.{eventId}.choice1,event.{eventId}.choice1.result");
            choices.AppendLine($"{eventId},2,event.{eventId}.choice2,event.{eventId}.choice2.result");

            conditions.AppendLine($"{eventId},,near_node,{nodeId},0");

            outcomes.AppendLine($"{eventId},1,unlock_road,{roadId},0");

            WriteLoc(loc, $"event.{eventId}.name", t.nameRu, t.nameEn);
            WriteLoc(loc, $"event.{eventId}.description", t.descRu, t.descEn);
            WriteLoc(loc, $"event.{eventId}.choice1", t.accept, t.acceptEn);
            WriteLoc(loc, $"event.{eventId}.choice1.result", t.acceptResultRu, t.acceptResultEn);
            WriteLoc(loc, $"event.{eventId}.choice2", t.decline, t.declineEn);
            WriteLoc(loc, $"event.{eventId}.choice2.result", t.declineResultRu, t.declineResultEn);
        }

        #endregion

        #region Quest

        private static readonly (string nameRu, string nameEn, string descRu, string descEn,
            string accept, string acceptEn, string acceptResultRu, string acceptResultEn,
            string decline, string declineEn, string declineResultRu, string declineResultEn)[] QuestTemplates =
        {
            (
                "Долг караванщика", "Caravan Master's Debt",
                "Бывалый караванщик догоняет ваш отряд. Он говорит, что обязан вам жизнью — в прошлый раз вы помогли его людям на переправе. В благодарность он готов открыть секретный маршрут, которым пользуются только избранные торговцы.",
                "A seasoned caravan master catches up with your party. He says he owes you his life — you helped his people at a crossing once before. In gratitude, he offers to reveal a secret route used only by select merchants.",
                "Принять благодарность", "Accept the gratitude",
                "Караванщик разворачивает потайную карту и подробно описывает каждый поворот. Теперь этот путь открыт и для вас!",
                "The caravan master unfolds a hidden map and describes every turn in detail. This route is now open to you as well!",
                "Вежливо отказать", "Politely decline",
                "Караванщик кланяется и уходит, унося свой секрет.",
                "The caravan master bows and departs, taking his secret with him."
            ),
            (
                "Благословение храма", "Temple's Blessing",
                "Настоятель придорожного храма узнаёт караван — вы когда-то пожертвовали щедрую сумму на восстановление. В благодарность он ведёт вас к тайной двери за алтарём, откуда начинается древний паломнический путь.",
                "The abbot of a roadside temple recognizes the caravan — you once donated generously to its restoration. In gratitude, he leads you to a hidden door behind the altar, where an ancient pilgrim's road begins.",
                "Войти", "Enter",
                "За дверью — высеченный в скале тоннель, выходящий на забытую, но пригодную дорогу. Монах благословляет вас в путь. Новый маршрут открыт!",
                "Behind the door, a tunnel carved into rock opens onto a forgotten but passable road. The monk blesses your journey. A new route unlocked!",
                "Поблагодарить и уйти", "Thank and leave",
                "Настоятель кивает с пониманием и закрывает дверь за алтарём.",
                "The abbot nods with understanding and closes the door behind the altar."
            ),
            (
                "Карта контрабандиста", "Smuggler's Map",
                "Ночью к лагерю подкрадывается тень. Это контрабандист, которого вы когда-то не выдали стражникам. Он протягивает свёрнутый пергамент — на нём отмечен тайный путь через горы, который он использовал годами.",
                "A shadow creeps toward the camp at night. It is a smuggler you once shielded from the guards. He extends a rolled parchment — on it, a secret mountain path he has used for years.",
                "Взять карту", "Take the map",
                "Пергамент покрыт точными отметками и символами. Контрабандист исчезает в ночи так же тихо, как пришёл. Тайный маршрут теперь ваш!",
                "The parchment is covered in precise markings and symbols. The smuggler vanishes into the night as silently as he came. The secret route is now yours!",
                "Отказаться", "Refuse",
                "Контрабандист молча прячет пергамент и растворяется в темноте.",
                "The smuggler silently hides the parchment and melts into the darkness."
            ),
        };

        private static void WriteQuestEvent(int roadIndex, string roadId, string nodeId,
            StringBuilder events, StringBuilder choices, StringBuilder conditions,
            StringBuilder outcomes, StringBuilder loc)
        {
            var t = QuestTemplates[roadIndex % QuestTemplates.Length];
            string eventId = $"road_quest_{roadId}";

            events.AppendLine($"{eventId},discovery,event.{eventId}.name,event.{eventId}.description,,,1,0");

            choices.AppendLine($"{eventId},1,event.{eventId}.choice1,event.{eventId}.choice1.result");
            choices.AppendLine($"{eventId},2,event.{eventId}.choice2,event.{eventId}.choice2.result");

            conditions.AppendLine($"{eventId},,near_node,{nodeId},0");

            outcomes.AppendLine($"{eventId},1,unlock_road,{roadId},0");

            WriteLoc(loc, $"event.{eventId}.name", t.nameRu, t.nameEn);
            WriteLoc(loc, $"event.{eventId}.description", t.descRu, t.descEn);
            WriteLoc(loc, $"event.{eventId}.choice1", t.accept, t.acceptEn);
            WriteLoc(loc, $"event.{eventId}.choice1.result", t.acceptResultRu, t.acceptResultEn);
            WriteLoc(loc, $"event.{eventId}.choice2", t.decline, t.declineEn);
            WriteLoc(loc, $"event.{eventId}.choice2.result", t.declineResultRu, t.declineResultEn);
        }

        #endregion

        private static void WriteLoc(StringBuilder sb, string key, string ru, string en)
        {
            sb.AppendLine($"{CsvEscape(key)},road_events,,{CsvEscape(ru)},{CsvEscape(en)}");
        }

        private static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        private static void WriteCsv(string fileName, StringBuilder content)
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), CSV_FOLDER);
            string filePath = Path.Combine(dir, fileName);
            File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
        }
    }
}
