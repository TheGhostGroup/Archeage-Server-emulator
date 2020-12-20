using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.NPChar;
using AAEmu.Game.Models.Game.Quests.Templates;

namespace AAEmu.Game.Models.Game.Quests.Acts
{
    public class QuestActConReportNpc : QuestActTemplate
    {
        public uint NpcId { get; set; }
        public bool UseAlias { get; set; }
        public uint QuestActObjAliasId { get; set; }

        public override bool Use(Character character, Quest quest, int objective)
        {
            _log.Debug("QuestActConReportNpc");

            if (!(character.CurrentTarget is Npc))
            {
                return false;
            }

            return ((Npc)character.CurrentTarget).TemplateId == NpcId;
        }
    }
}
