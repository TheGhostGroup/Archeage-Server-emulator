using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using AAEmu.Commons.Utils;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.AI.Abstracts;
using AAEmu.Game.Models.Game.AI.Static;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.NPChar;
using AAEmu.Game.Models.Game.Transfers.Paths;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Models.Game.Units.Movements;
using AAEmu.Game.Models.Game.Units.Route;
using AAEmu.Game.Models.Game.World;
using AAEmu.Game.Utils;

/*
   Author:Sagara, NLObP
*/
namespace AAEmu.Game.Models.Game.AI
{
    public sealed class NpcAi : ACreatureAi
    {
        public double Angle { get; set; }
        //public float Distance { get; set; } = 0f;
        //public float movingDistance { get; set; } = 0.27f;
        //public double AngleTmp { get; set; }
        //public float AngVelocity { get; set; } = 45.0f;
        //public float MaxVelocityForward { get; set; } = 5.4f;
        //public float MaxVelocityBackward { get; set; } = 0f;
        //public float VelAccel { get; set; } = 1.8f;
        //public Vector3 vMovingDistance { get; set; } = new Vector3();
        //public Vector3 vMaxVelocityForwardRun { get; set; } = new Vector3(5.4f, 5.4f, 5.4f);
        //public Vector3 vMaxVelocityForwardWalk { get; set; } = new Vector3(1.8f, 1.8f, 1.8f);
        public Vector3 Velocity { get; set; } = new Vector3();
        //public Vector3 vMaxVelocityBackward { get; set; } = new Vector3();
        //public Vector3 vVelAccel { get; set; } = new Vector3(1.8f, 1.8f, 1.8f);
        //public Vector3 vPosition { get; set; } = new Vector3();
        //public Vector3 vTarget { get; set; } = new Vector3();
        //public Vector3 vDistance { get; set; } = new Vector3();
        //public float RangeToCheckPoint { get; set; } = 1.0f; // distance to checkpoint at which it is considered that we have reached it
        //public Vector3 vRangeToCheckPoint { get; set; } = new Vector3(1.0f, 1.0f, 0f); // distance to checkpoint at which it is considered that we have reached it

        public NpcAi(GameObject owner, float visibleRange) : base(owner, visibleRange)
        {
        }

        protected override void IamSeeSomeone(GameObject someone)
        {
            switch (someone.UnitType)
            {
                case BaseUnitType.Character:
                    var chr = (Character)someone;
                    var npc = (Npc)Owner;
                    var target = (BaseUnit)someone;
                    if (!npc.IsInBattle && npc.Hp > 0)
                    {
                        // Monstrosity & Hostile & Fish
                        if (npc.Faction.Id == 115 || npc.Faction.Id == 3 || npc.Faction.Id == 172)
                        {
                            // if the Npc is aggressive, he will look at us and attack if close to us, otherwise he just looks at us
                            if (npc.Template.Aggression && npc.Template.AggroLinkHelpDist * npc.Template.AttackStartRangeScale > Math.Abs(MathUtil.CalculateDistance(npc.Position, chr.Position, true)))
                            {
                                // NPC attacking us
                                //npc.Patrol = null;
                                //npc.Patrol?.Pause(npc);

                                // AiAggro(ai_commands = 4065, count=0)
                                chr.BroadcastPacket(new SCTargetChangedPacket(npc.ObjId, chr.ObjId), true);
                                chr.BroadcastPacket(new SCAiAggroPacket(npc.ObjId, 1, chr.ObjId), true);
                                chr.BroadcastPacket(new SCCombatEngagedPacket(npc.ObjId), true); // caster
                                chr.BroadcastPacket(new SCAggroTargetChangedPacket(npc.ObjId, chr.ObjId), true);
                                npc.CurrentTarget = target;
                                npc.SetForceAttack(true);
                                npc.IsAutoAttack = true;
                                npc.IsInBattle = true;
                                var combat = new Combat();
                                //npc.Patrol.UpdateTime = DateTime.Now;
                                combat.Execute(npc);
                            }
                            else if (Math.Abs(MathUtil.CalculateDistance(npc.Position, chr.Position, true)) < 10f) // preferredCombatDistance = 20
                            {
                                //npc.Patrol = null;
                                //npc.Patrol?.Pause(npc);

                                // Npc looks at us
                                if (npc.CurrentTarget != target)
                                {
                                    chr.BroadcastPacket(new SCTargetChangedPacket(npc.ObjId, chr.ObjId), true);
                                    npc.CurrentTarget = target;
                                }
                                var seq = (uint)(DateTime.Now - GameService.StartTime).TotalMilliseconds;
                                var moveType = (ActorData)UnitMovement.GetType(UnitMovementType.Actor);

                                moveType.X = npc.Position.X;
                                moveType.Y = npc.Position.Y;
                                moveType.Z = AppConfiguration.Instance.HeightMapsEnable ? WorldManager.Instance.GetHeight(npc.Position.ZoneId, npc.Position.X, npc.Position.Y) : npc.Position.Z;
                                if (npc.Position.Z - moveType.Z > 2.0)
                                {
                                    moveType.Z = npc.Position.Z;
                                }
                                // looks in the direction of movement
                                Angle = MathUtil.CalculateAngleFrom(npc, chr);
                                var rotZ = MathUtil.ConvertDegreeToDirection(Angle);
                                moveType.Rot = new Quaternion(0f, 0f, Helpers.ConvertDirectionToRadian(rotZ), 1f);
                                npc.Rot = moveType.Rot;

                                moveType.DeltaMovement = Vector3.Zero;
                                Velocity = Vector3.Zero;

                                moveType.Stance = 1;    //combat=0, idle=1
                                moveType.Alertness = 1; //idle=0, alert = 1, combat=2
                                moveType.Time = seq;
                                chr.BroadcastPacket(new SCOneUnitMovementPacket(npc.ObjId, moveType), true);
                            }
                        }
                        else if (npc.SimulationNpc != null && npc.SimulationNpc.FollowPath)
                        {
                            npc.SimulationNpc.GoToPath(npc, true);
                        }
                        else if (npc.Template.AiFileId == AiFilesType.Roaming || npc.Template.AiFileId == AiFilesType.BigMonsterRoaming || npc.Template.AiFileId == AiFilesType.ArcherRoaming || npc.Template.AiFileId == AiFilesType.WildBoarRoaming)
                        {   // Npc roams around the spawn point in random directions
                            if (npc.CurrentTarget != null)
                            {
                                chr.BroadcastPacket(new SCTargetChangedPacket(npc.ObjId, 0), true);
                                npc.CurrentTarget = null;
                            }
                            if (npc.Patrol == null)
                            {
                                npc.IsInBattle = false;
                                npc.Patrol = new Roaming { Interrupt = true, Loop = true, Abandon = false };
                                npc.Patrol.Interrupt = true; // можно прервать
                                npc.Patrol.Loop = true;      // повторять в цикле
                                npc.Patrol.Abandon = false;  // не прерванный
                                npc.Patrol.Pause(npc);
                                npc.Patrol.LastPatrol = null; // предыдущего патруля нет
                                npc.Patrol.Recovery(npc);     // запустим патруль
                            }
                            else
                            {
                                npc.Patrol.Recovery(npc);
                            }
                        }
                        //// использование путей из логов с помощью файла npc_paths.json
                        ////if (
                        ////    npc.TemplateId == 11999
                        ////    || npc.TemplateId == 8172
                        ////    || npc.TemplateId == 8176
                        ////    || npc.TemplateId == 3576
                        ////    || npc.TemplateId == 918
                        ////    || npc.TemplateId == 3626
                        ////    || npc.TemplateId == 7660
                        ////    || npc.TemplateId == 12143
                        ////    )
                        //{
                        //    if (!npc.IsInPatrol)
                        //    {
                        //        var path = new SimulationNpc(npc);
                        //        // организуем последовательность "Дорог" для следования "Гвардов"
                        //        var lnpp = new List<NpcsPathPoint>();
                        //        foreach (var np in NpcsPath.NpcsPaths)
                        //        {
                        //            if (np.ObjId != npc.ObjId) { continue; }

                        //            foreach (var npp in np.Pos)
                        //            {
                        //                lnpp.Add(npp);
                        //            }
                        //            path.NpcsRoutes.TryAdd(npc.TemplateId, lnpp);
                        //            break;
                        //        }

                        //        var go = true;
                        //        if (path.NpcsRoutes.Count == 0)
                        //        {
                        //            go = false;
                        //        }
                        //        else
                        //        {
                        //            if (path.NpcsRoutes.Any(route => route.Value.Count == 0))
                        //            {
                        //                go = false;
                        //            }
                        //        }
                        //        //if (path.Routes2.Count != 0)
                        //        if (go)
                        //        {
                        //            path.LoadNpcPathFromNpcsRoutes(npc.TemplateId); // начнем с самого начала
                        //            //_log.Warn("TransfersPath #" + transfer.TemplateId);
                        //                                                            //_log.Warn("First spawn myX=" + transfer.Position.X + " myY=" + transfer.Position.Y + " myZ=" + transfer.Position.Z + " rotZ=" + transfer.Rot.Z + " rotationZ=" + transfer.Position.RotationZ);
                        //            npc.IsInPatrol = true; // so as not to run the route a second time

                        //            path.GoToPath(npc, true);
                        //        }
                        //        else
                        //        {
                        //            //_log.Warn("PathName: " + npc.Template.TransferPaths[0].PathName + " not found!");
                        //        }
                        //    }
                        //}
                    }
                    break;
                case BaseUnitType.Npc:
                    break;
                case BaseUnitType.Slave:
                    break;
                case BaseUnitType.Housing:
                    break;
                case BaseUnitType.Transfer:
                    break;
                case BaseUnitType.Mate:
                    break;
                case BaseUnitType.Shipyard:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void IamUnseeSomeone(GameObject someone)
        {
        }

        protected override void SomeoneSeeMe(GameObject someone)
        {
        }

        protected override void SomeoneUnseeMee(GameObject someone)
        {
        }

        protected override void SomeoneThatIamSeeWasMoved(GameObject someone, MovementAction action)
        {
        }

        protected override void SomeoneThatSeeMeWasMoved(GameObject someone, MovementAction action)
        {
        }
    }
}
