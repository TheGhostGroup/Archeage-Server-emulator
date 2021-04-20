using System.Numerics;

using AAEmu.Commons.Network;
using AAEmu.Commons.Utils;
using AAEmu.Game.Models.Game.World;

namespace AAEmu.Game.Models.Game.Units.Movements
{
    public enum EStance : sbyte
    {
        Null = -1,
        Combat = 0,
        Idle = 1,
        Swim = 2,
        Coswim = 3,
        Zerog = 4,
        Stealth = 5,
        Climb = 6,
        Prone = 7,
        Fly = 8,
        Last = 9
    }
    public enum AiAlertness : sbyte
    {
        Idle = 0,
        Alert = 1,
        Combat = 2
    }
    public enum ActorMoveType : ushort
    {
        StandStill = 3,
        Run = 4,
        Walk = 5,
    }


    public class ActorData : UnitMovement
    {
        public Vector3 DeltaMovement { get; set; }
        // ---
        //public sbyte[] DeltaMovement { get; set; }
        // ---
        public ushort FallVel { get; set; }
        public EStance Stance { get; set; }
        public AiAlertness Alertness { get; set; }
        public byte GcFlags { get; set; }
        public ushort GcPart { get; set; }
        public ushort GcPartId { get; set; }
        public uint GcId { get; set; }
        public WorldPos GcWorldPos { get; set; }
        // +++
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public float Z2 { get; set; }
        // +++
        public Quaternion GcWorldRot { get; set; }
        // ---
        //public sbyte RotationX2 { get; set; }
        //public sbyte RotationY2 { get; set; }
        //public sbyte RotationZ2 { get; set; }
        // ---
        public uint ClimbData { get; set; }
        public uint MaxPushedUnitId { get; set; }

        public ActorData()
        {
            WorldPos = new WorldPos(Helpers.ConvertLongX(X), Helpers.ConvertLongY(Y), Z);
            GcWorldPos = new WorldPos(Helpers.ConvertLongX(X2), Helpers.ConvertLongY(Y2), Z2);
        }

        public override void Read(PacketStream stream)
        {
            base.Read(stream);
            (X, Y, Z) = stream.ReadPositionBc();
            WorldPos = new WorldPos(Helpers.ConvertLongX(X), Helpers.ConvertLongY(Y), Z);

            //VelX = stream.ReadInt16();
            //VelY = stream.ReadInt16();
            //VelZ = stream.ReadInt16();
            var tempVelocity = stream.ReadVector3Short();
            Velocity = new Vector3(tempVelocity.X * 60f, tempVelocity.Y * 60f, tempVelocity.Z * 60f);
            VelX = (short)Velocity.X;
            VelY = (short)Velocity.Y;
            VelZ = (short)Velocity.Z;

            //RotationX = stream.ReadSByte();
            //RotationY = stream.ReadSByte();
            //RotationZ = stream.ReadSByte();
            Rot = stream.ReadQuaternionSbyte();
            RotationX = 0;
            RotationY = 0;
            RotationZ = Helpers.ConvertRadianToSbyteDirection(Rot.Z);

            //DeltaMovement = new sbyte[3];
            //DeltaMovement[0] = stream.ReadSByte();
            //DeltaMovement[1] = stream.ReadSByte();
            //DeltaMovement[2] = stream.ReadSByte();
            DeltaMovement = stream.ReadVector3Sbyte();

            Stance = (EStance)stream.ReadSByte();
            Alertness = (AiAlertness)stream.ReadSByte();
            actorFlags = (ActorMoveType)stream.ReadUInt16(); // ushort in 3.0.3.0, sbyte in 1.2
            //if ((short)actorFlags  < 0) // TODO если падает и ударяется об землю, видимо Значение нужно вычитать от текущего HP
            if (((ushort)actorFlags & 0x8000) == 0x8000)
            {
                FallVel = stream.ReadUInt16(); // actor.fallVel
            }

            if (((ushort)actorFlags & 0x20) == 0x20) // TODO если находится на движущейся повозке/лифте/корабле, то здесь координаты персонажа
            {
                GcFlags = stream.ReadByte();    // actor.gcFlags
                GcPart = stream.ReadUInt16(); // actor.gcPart
                GcPartId = stream.ReadUInt16(); // actor.gcPartId
                (X2, Y2, Z2) = stream.ReadPositionBc(); // ix, iy, iz
                GcWorldPos = new WorldPos(Helpers.ConvertLongX(X2), Helpers.ConvertLongY(Y2), Z2);
                //var (x2, y2, z2) = stream.ReadWorldPosition();
                //GcWorldPos = new WorldPos(x2, y2, z2);

                //RotationX2 = stream.ReadSByte();
                //RotationY2 = stream.ReadSByte(); 
                //RotationZ2 = stream.ReadSByte();
                GcWorldRot = stream.ReadQuaternionSbyte();
            }
            if (((ushort)actorFlags & 0x60) != 0)
            {
                GcId = stream.ReadUInt32(); // actor.gcId
            }

            if (((ushort)actorFlags & 0x40) == 0x40)
            {
                ClimbData = stream.ReadUInt32(); // actor.climbData
            }
            if (((ushort)actorFlags & 0x100) == 0x100)
            {
                MaxPushedUnitId = stream.ReadUInt32(); // actor.maxPushedUnitId
            }
        }

        public override PacketStream Write(PacketStream stream)
        {
            base.Write(stream);
            stream.WritePositionBc(X, Y, Z);
            WorldPos = new WorldPos(Helpers.ConvertLongX(X), Helpers.ConvertLongY(Y), Z);
            //stream.WriteWorldPosition(WorldPos.X, WorldPos.Y, WorldPos.Z);

            //stream.Write(VelX);
            //stream.Write(VelY);
            //stream.Write(VelZ);
            var tempVelocity = new Vector3(Velocity.X / 60f, Velocity.Y / 60f, Velocity.Z / 60f);
            stream.WriteVector3Short(tempVelocity);

            //stream.Write(RotationX);
            //stream.Write(RotationY);
            //stream.Write(RotationZ);
            stream.WriteQuaternionSbyte(Rot);

            //stream.Write(DeltaMovement[0]);
            //stream.Write(DeltaMovement[1]);
            //stream.Write(DeltaMovement[2]);
            stream.WriteVector3Sbyte(DeltaMovement);

            stream.Write((sbyte)Stance);
            stream.Write((sbyte)Alertness);
            stream.Write((ushort)actorFlags);
            if (((ushort)actorFlags & 0x8000) == 0x8000)
            {
                stream.Write(FallVel);
            }

            if (((ushort)actorFlags & 0x20) == 0x20)
            {
                stream.Write(GcFlags);
                stream.Write(GcPart);
                stream.Write(GcPartId);

                stream.WritePositionBc(X2, Y2, Z2);
                GcWorldPos = new WorldPos(Helpers.ConvertLongX(X2), Helpers.ConvertLongY(Y2), Z2);
                //stream.WriteWorldPosition(GcWorldPos.X, GcWorldPos.Y, GcWorldPos.Z);

                //stream.Write(RotationX2);
                //stream.Write(RotationY2);
                //stream.Write(RotationZ2);
                stream.WriteQuaternionSbyte(GcWorldRot);

            }
            if (((ushort)actorFlags & 0x60) != 0)
            {
                stream.Write(GcId);
            }

            if (((ushort)actorFlags & 0x40) == 0x40)
            {
                stream.Write(ClimbData);
            }
            if (((ushort)actorFlags & 0x100) == 0x100)
            {
                stream.Write(MaxPushedUnitId);
            }

            return stream;
        }
    }
}
