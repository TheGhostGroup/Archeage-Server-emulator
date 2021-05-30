using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Models.Game.Skills;

namespace AAEmu.Game.Core.Packets.G2C
{
    public class SCSkillFiredPacket : GamePacket
    {
        private readonly uint _id;
        private readonly ushort _tl;
        private readonly SkillCaster _caster;
        private readonly SkillCastTarget _target;
        private readonly SkillObject _skillObject;
        private readonly Skill _skill;

        private readonly short _effectDelay = 37;
        private readonly int _fireAnimId = 2;
        private readonly bool _dist;
        public short ComputedDelay { get; set; }

        public SCSkillFiredPacket(uint id, ushort tl, SkillCaster caster, SkillCastTarget target, Skill skill, SkillObject skillObject)
            : base(SCOffsets.SCSkillFiredPacket, 5)
        {
            _id = id;
            _tl = tl;
            _caster = caster;
            _target = target;
            _skill = skill;
            _skillObject = skillObject;
            var totalDelay = 0;
            if (skill.Template.EffectDelay > 0)
                totalDelay += skill.Template.EffectDelay;
            //if (skill.Template.EffectSpeed > 0)
            //    totalDelay += (int) ((caster.GetDistanceTo(target) / skill.Template.EffectSpeed) * 1000.0f);
            //if (skill.Template.FireAnim != null && skill.Template.UseAnimTime)
            //    totalDelay += (int)(skill.Template.FireAnim.CombatSyncTime * (caster.GlobalCooldownMul / 100));

        }

        public SCSkillFiredPacket(uint id, ushort tl, SkillCaster caster, SkillCastTarget target, Skill skill, SkillObject skillObject, short effectDelay = 37, int fireAnimId = 2, bool dist = true)
            : base(SCOffsets.SCSkillFiredPacket, 5)
        {
            _id = id;
            _tl = tl;
            _caster = caster;
            _target = target;
            _skill = skill;
            _skillObject = skillObject;
            _effectDelay = effectDelay;
            _fireAnimId = fireAnimId;
            _dist = dist;
        }

        public override PacketStream Write(PacketStream stream)
        {
            stream.Write(_id);
            stream.Write(_tl);
            stream.Write(_caster);
            stream.Write(_target);
            stream.Write(_skillObject);

            stream.Write((short)(ComputedDelay / 10));                       // msec TODO +10 It became visible flying arrows
            stream.Write((short)(_skill.Template.ChannelingTime / 10 + 10)); // msec
            stream.Write((byte)0);                                           // f [c, e, p]
            stream.WritePisc(_skill.Template.FireAnim?.Id ?? 0, 0); // fire_anim_id 
            stream.Write((byte)0); // flag

            return stream;
        }
    }
}
