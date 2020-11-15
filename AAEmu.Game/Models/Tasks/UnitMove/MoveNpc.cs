using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.NPChar;
using AAEmu.Game.Models.Game.Skills;
using AAEmu.Game.Models.Game.Skills.Static;
using AAEmu.Game.Models.Game.Transfers.Paths;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Models.Game.Units.Route;

namespace AAEmu.Game.Models.Tasks.UnitMove
{
    public class MoveNpc : Task
    {
        private readonly SimulationNpc _patrol;
        private readonly Unit _unit;
        private readonly float _targetX;
        private readonly float _targetY;
        private readonly float _targetZ;
        private readonly NpcsPathPoint _pp;
        private readonly bool _useSkill;

        /// <summary>
        /// Move task
        /// </summary>
        /// <param name="patrol"></param>
        /// <param name="unit"></param>
        /// <param name="TargetX"></param>
        /// <param name="TargetY"></param>
        /// <param name="TargetZ"></param>
        /// <param name="pp"></param>
        /// <param name="useSkill"></param>
        public MoveNpc(SimulationNpc patrol, Unit unit, float TargetX, float TargetY, float TargetZ, NpcsPathPoint pp = null, bool useSkill = false)
        {
            _patrol = patrol;
            _targetX = TargetX;
            _targetY = TargetY;
            _targetZ = TargetZ;
            _unit = unit;
            _pp = pp;
            _useSkill = useSkill;

            if (_useSkill)
            {
                //1)
                uint skillId = 19658; // начать рубить дерево для Woodcutter Solace
                var skill1 = new Skill(SkillManager.Instance.GetSkillTemplate(skillId));
                var casterType = SkillCaster.GetByType(EffectOriginType.Skill); // who uses
                casterType.ObjId = unit.ObjId;
                var targetType = patrol.GetSkillCastTarget(unit, skill1);
                var flag = 0;
                var flagType = flag & 15;
                var skillObject = SkillObject.GetByType((SkillObjectType)flagType);
                skill1.Use(unit, casterType, targetType, skillObject);
            }
        }

        /// <summary>
        /// Perform tasks
        /// </summary>
        public override void Execute()
        {
            switch (_unit)
            {
                case Npc npc:
                    if (_useSkill)
                    {
                        //2)
                        uint skillId = 19412; // закончить рубить дерево для Woodcutter Solace
                        var skill2 = new Skill(SkillManager.Instance.GetSkillTemplate(skillId));
                        var casterType = SkillCaster.GetByType(EffectOriginType.Skill); // who uses
                        casterType.ObjId = npc.ObjId;
                        var targetType = _patrol.GetSkillCastTarget(npc, skill2);
                        var flag = 0;
                        var flagType = flag & 15;
                        var skillObject = SkillObject.GetByType((SkillObjectType)flagType);
                        skill2.Use(npc, casterType, targetType, skillObject);
                    }

                    _patrol?.MoveToPathNpc(_patrol, npc, _targetX, _targetY, _targetZ, _pp);
                    break;
            }
        }
    }
}
