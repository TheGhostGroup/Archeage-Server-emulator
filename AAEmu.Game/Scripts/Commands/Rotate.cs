using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.Id;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Core.Managers.UnitManagers;
using AAEmu.Game.Models.Game.Units.Movements;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.DoodadObj;
using AAEmu.Game.Models.Game.NPChar;
using AAEmu.Game.Models.Game.World;
using AAEmu.Game.Utils;
using AAEmu.Commons.Utils;
using NLog;
using System;
using System.Numerics;

namespace AAEmu.Game.Scripts.Commands
{
    public class Rotate : ICommand
    {
        protected static Logger _log = LogManager.GetCurrentClassLogger();
        public void OnLoad()
        {
            CommandManager.Instance.Register("rotate", this);
        }

        public string GetCommandLineHelp()
        {
            return "<npc||doodad> <objId>";
        }

        public string GetCommandHelpText()
        {
            return "Rotate target unit towards you";
        }

        public void Execute(Character character, string[] args)
        {
            //if (args.Length < 2)
            //{
            //    character.SendMessage("[Rotate] /rotate <objType: npc, doodad> <objId>");
            //    return;
            //}

            if (character.CurrentTarget != null)
            {
                character.SendMessage("[Rotate] Unit: {0}, ObjId: {1}", character.CurrentTarget.Name, character.CurrentTarget.ObjId);

                var Seq = (uint)(DateTime.UtcNow - GameService.StartTime).TotalMilliseconds;
                var moveType = (ActorData)UnitMovement.GetType(UnitMovementType.Actor);

                moveType.X = character.CurrentTarget.Position.X;
                moveType.Y = character.CurrentTarget.Position.Y;
                moveType.Z = character.CurrentTarget.Position.Z;

                var angle = MathUtil.CalculateAngleFrom(character.CurrentTarget, character);
                var rotZ = MathUtil.ConvertDegreeToDirection(angle);

                //var direction = new Vector3();
                //if (vDistance != Vector3.Zero)
                //    direction = Vector3.Normalize(vDistance);
                ////var rotation = (float)Math.Atan2(direction.Y, direction.X);

                //moveType.Rot = Quaternion.CreateFromAxisAngle(direction, rotZ);
                moveType.Rot = new Quaternion(0f, 0f, Helpers.ConvertDirectionToRadian(rotZ), 1f);
                moveType.DeltaMovement = Vector3.Zero;

                moveType.actorFlags = ActorMoveType.Walk; // 5-walk, 4-run, 3-stand still
                moveType.Stance = EStance.Idle;           // COMBAT = 0x0, IDLE = 0x1
                moveType.Alertness = AiAlertness.Idle;    // IDLE = 0x0, ALERT = 0x1, COMBAT = 0x2
                moveType.Time = Seq;                      // has to change all the time for normal motion.

                character.BroadcastPacket(new SCOneUnitMovementPacket(character.CurrentTarget.ObjId, moveType), true);
            }
            else
                character.SendMessage("[Rotate] You need to target something first");
        }
    }
}
