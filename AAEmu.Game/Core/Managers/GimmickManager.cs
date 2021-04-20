using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using AAEmu.Commons.Utils;
using AAEmu.Game.Core.Managers.Id;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.Faction;
using AAEmu.Game.Models.Game.Gimmicks;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Utils.DB;

using NLog;

namespace AAEmu.Game.Core.Managers
{
    public class GimmickManager : Singleton<GimmickManager>
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private Dictionary<uint, GimmickTemplate> _templates;
        private Dictionary<uint, Gimmick> _activeGimmicks;
        public Thread thread { get; set; }
        private bool ThreadRunning = true;

        public bool Exist(uint templateId)
        {
            return _templates.ContainsKey(templateId);
        }

        public GimmickTemplate GetGimmickTemplate(uint id)
        {
            return _templates.ContainsKey(id) ? _templates[id] : null;
        }

        public Gimmick Create(uint objectId, uint templateId, GimmickSpawner spawner)
        {
            if (!_templates.ContainsKey(templateId))
            {
                return null;
            }

            var template = _templates[templateId];
            //var template = GetGimmickTemplate(templateId);

            var gimmick = new Gimmick
            {
                ObjId = objectId > 0 ? objectId : ObjectIdManager.Instance.GetNextId(),
                Spawner = spawner,
                Template = template
            };
            gimmick.GimmickId = gimmick.ObjId;
            gimmick.TemplateId = template.Id;
            gimmick.Faction = new SystemFaction();
            gimmick.ModelPath = template.ModelPath;
            gimmick.Patrol = null;
            gimmick.Position = spawner.Position.Clone();

            gimmick.WorldPos.X = Helpers.ConvertLongY(gimmick.Position.X);
            gimmick.WorldPos.Y = Helpers.ConvertLongY(gimmick.Position.Y);
            gimmick.WorldPos.Z = gimmick.Position.Z;

            gimmick.Vel = new Vector3(0f, 0f, 0f);
            gimmick.Rot = new Quaternion(spawner.RotationX, spawner.RotationY, spawner.RotationZ, spawner.RotationW);
            gimmick.ModelParams = new UnitCustomModelParams();

            gimmick.Spawn();
            _activeGimmicks.Add(gimmick.ObjId, gimmick);

            return gimmick;
        }

        public void Load()
        {
            _templates = new Dictionary<uint, GimmickTemplate>();
            _activeGimmicks = new Dictionary<uint, Gimmick>();

            _log.Info("Loading gimmick templates...");

            #region SQLLite
            using (var connection = SQLite.CreateConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM gimmicks";
                    command.Prepare();
                    using (var reader = new SQLiteWrapperReader(command.ExecuteReader()))
                    {
                        while (reader.Read())
                        {
                            var template = new GimmickTemplate
                            {
                                Id = reader.GetUInt32("id"), // GimmickId
                                AirResistance = reader.GetFloat("air_resistance"),
                                CollisionMinSpeed = reader.GetFloat("collision_min_speed"),
                                //template.CollisionSkillId = reader.GetUInt32("collision_skill_id");
                                //template.CollisionSkillId = reader.IsDBNull("collision_skill_id") ? 0 : reader.GetUInt32("collision_skill_id");
                                CollisionSkillId = reader.GetUInt32("collision_skill_id", 0),

                                CollisionUnitOnly = reader.GetBoolean("collision_unit_only"),
                                Damping = reader.GetFloat("damping"),
                                Density = reader.GetFloat("density"),
                                DisappearByCollision = reader.GetBoolean("disappear_by_collision"),
                                FadeInDuration = reader.GetUInt32("fade_in_duration"),
                                FadeOutDuration = reader.GetUInt32("fade_out_duration"),
                                FreeFallDamping = reader.GetFloat("free_fall_damping"),
                                Graspable = reader.GetBoolean("graspable"),
                                Gravity = reader.GetFloat("gravity"),
                                LifeTime = reader.GetUInt32("life_time"),
                                Mass = reader.GetFloat("mass"),
                                ModelPath = reader.GetString("model_path"),
                                Name = reader.GetString("name"),
                                NoGroundCollider = reader.GetBoolean("no_ground_collider"),
                                PushableByPlayer = reader.GetBoolean("pushable_by_player"),
                                SkillDelay = reader.GetUInt32("skill_delay"),
                                //template.SkillId = reader.GetUInt32("skill_id");
                                //template.CollisionSkillId = reader.IsDBNull("skill_id") ? 0 : reader.GetUInt32("skill_id");
                                SkillId = reader.GetUInt32("skill_id", 0),

                                SpawnDelay = reader.GetUInt32("spawn_delay"),
                                WaterDamping = reader.GetFloat("water_damping"),
                                WaterDensity = reader.GetFloat("water_density"),
                                WaterResistance = reader.GetFloat("water_resistance")
                            };

                            _templates.Add(template.Id, template);
                        }
                    }
                }
            }
            #endregion
        }
        public void Initialize()
        {
            thread = new Thread(GimmickThread);
            thread.Start();
        }

        private Gimmick[] GetActiveGimmicks()
        {
            return _activeGimmicks.Values.ToArray();
        }

        private void GimmickThread()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                Thread.Sleep(50);
                var activeGimmicks = Instance.GetActiveGimmicks();
                foreach (var gimmick in activeGimmicks)
                {
                    GimmickTick(gimmick);
                }
            }
        }

        private static void GimmickTick(Gimmick gimmick)
        {
            if (gimmick.TimeLeft > 0)
            {
                return;
            }
            var maxVelocityForward = 4.5f;
            var maxVelocityBackward = -4.5f;
            var deltaTime = 0.05f;
            var movingDistance = 0.27f;

            Vector3 vPosition;
            Vector3 vTarget;
            Vector3 vDistance;

            var velocityZ = gimmick.Vel.Z;
            var pozitionZ = gimmick.Position.Z;
            vPosition = new Vector3(gimmick.Position.X, gimmick.Position.Y, pozitionZ);

            if (gimmick.Spawner.MiddleZ > 0)
            {
                if (pozitionZ < gimmick.Spawner.MiddleZ && gimmick.Vel.Z >= 0 && !gimmick.moveDown)
                {
                    vTarget = new Vector3(gimmick.Position.X, gimmick.Position.Y, gimmick.Spawner.MiddleZ);
                    vDistance = vTarget - vPosition; // dx, dy, dz
                    velocityZ = maxVelocityForward;

                    movingDistance = velocityZ * deltaTime;

                    if (Math.Abs(vDistance.Z) >= Math.Abs(movingDistance))
                    {
                        pozitionZ += movingDistance;
                        gimmick.Vel = new Vector3(gimmick.Vel.X, gimmick.Vel.Y, velocityZ);
                    }
                    else
                    {
                        pozitionZ = vTarget.Z;
                        gimmick.Vel = new Vector3(0f, 0f, 0f);
                    }
                }
                else if (vPosition.Z < gimmick.Spawner.TopZ && gimmick.Vel.Z >= 0 && !gimmick.moveDown)
                {
                    vTarget = new Vector3(gimmick.Position.X, gimmick.Position.Y, gimmick.Spawner.TopZ);
                    vDistance = vTarget - vPosition; // dx, dy, dz
                    velocityZ = maxVelocityForward;
                    movingDistance = velocityZ * deltaTime;

                    if (Math.Abs(vDistance.Z) >= Math.Abs(movingDistance))
                    {
                        pozitionZ += movingDistance;
                        gimmick.Vel = new Vector3(gimmick.Vel.X, gimmick.Vel.Y, velocityZ);
                        gimmick.moveDown = false;
                    }
                    else
                    {
                        pozitionZ = vTarget.Z;
                        gimmick.Vel = new Vector3(0f, 0f, 0f);
                        gimmick.moveDown = true;
                    }
                }
                else if (vPosition.Z > gimmick.Spawner.MiddleZ && gimmick.Vel.Z <= 0 && gimmick.moveDown)
                {
                    vTarget = new Vector3(gimmick.Position.X, gimmick.Position.Y, gimmick.Spawner.MiddleZ);
                    vDistance = vTarget - vPosition; // dx, dy, dz
                    velocityZ = maxVelocityBackward;
                    movingDistance = velocityZ * deltaTime;

                    if (Math.Abs(vDistance.Z) >= Math.Abs(movingDistance))
                    {
                        pozitionZ += movingDistance;
                        gimmick.Vel = new Vector3(gimmick.Vel.X, gimmick.Vel.Y, velocityZ);
                    }
                    else
                    {
                        pozitionZ = vTarget.Z;
                        gimmick.Vel = new Vector3(0f, 0f, 0f);
                    }
                }
                else
                {
                    vTarget = new Vector3(gimmick.Position.X, gimmick.Position.Y, gimmick.Spawner.BottomZ);
                    vDistance = vTarget - vPosition; // dx, dy, dz
                    velocityZ = maxVelocityBackward;
                    movingDistance = velocityZ * deltaTime;

                    if (Math.Abs(vDistance.Z) >= Math.Abs(movingDistance))
                    {
                        pozitionZ += movingDistance;
                        gimmick.Vel = new Vector3(gimmick.Vel.X, gimmick.Vel.Y, velocityZ);
                        gimmick.moveDown = true;
                    }
                    else
                    {
                        pozitionZ = vTarget.Z;
                        gimmick.Vel = new Vector3(0f, 0f, 0f);
                        gimmick.moveDown = false;
                    }
                }
            }
            else
            {
                if (vPosition.Z < gimmick.Spawner.TopZ && gimmick.Vel.Z >= 0)
                {
                    vTarget = new Vector3(gimmick.Position.X, gimmick.Position.Y, gimmick.Spawner.TopZ);
                    vDistance = vTarget - vPosition; // dx, dy, dz
                    velocityZ = maxVelocityForward;

                    movingDistance = velocityZ * deltaTime;

                    if (Math.Abs(vDistance.Z) >= Math.Abs(movingDistance))
                    {
                        pozitionZ += movingDistance;
                        gimmick.Vel = new Vector3(gimmick.Vel.X, gimmick.Vel.Y, velocityZ);
                    }
                    else
                    {
                        pozitionZ = vTarget.Z;
                        gimmick.Vel = new Vector3(0f, 0f, 0f);
                    }
                }
                else
                {
                    vTarget = new Vector3(gimmick.Position.X, gimmick.Position.Y, gimmick.Spawner.BottomZ);
                    vDistance = vTarget - vPosition; // dx, dy, dz
                    velocityZ = maxVelocityBackward;
                    movingDistance = velocityZ * deltaTime;

                    if (Math.Abs(vDistance.Z) >= Math.Abs(movingDistance))
                    {
                        pozitionZ += movingDistance;
                        gimmick.Vel = new Vector3(gimmick.Vel.X, gimmick.Vel.Y, velocityZ);
                    }
                    else
                    {
                        pozitionZ = vTarget.Z;
                        gimmick.Vel = new Vector3(0f, 0f, 0f);
                    }
                }
            }

            gimmick.Position.Z = pozitionZ;
            //gimmick.Vel = new Vector3(gimmick.Vel.X, gimmick.Vel.Y, velocityZ);

            // If the number of executions is less than the angle, continue adding tasks or stop moving
            if (Math.Abs(gimmick.Vel.Z) > 0)
            {
                gimmick.Time += 50;    // has to change all the time for normal motion.
                gimmick.BroadcastPacket(new SCGimmickMovementPacket(gimmick), true);
            }
            else
            {
                // stop for a few seconds
                gimmick.Time += 50;    // has to change all the time for normal motion.
                gimmick.BroadcastPacket(new SCGimmickMovementPacket(gimmick), true);
                gimmick.WaitTime = DateTime.Now.AddSeconds(gimmick.Spawner.WaitTime);
            }
        }

        internal void Stop()
        {
            ThreadRunning = false;
        }
    }
}

