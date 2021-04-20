using System.Numerics;

using AAEmu.Commons.Network;
using AAEmu.Commons.Utils;
using AAEmu.Game.Models.Game.World;

namespace AAEmu.Game.Models.Game.Units.Movements
{
    public class DefaultUnitMovement : UnitMovement
    {
        public override void Read(PacketStream stream)
        {
            base.Read(stream);
            (X, Y, Z) = stream.ReadPositionBc();
            WorldPos = new WorldPos(Helpers.ConvertLongX(X), Helpers.ConvertLongY(Y), Z);

            //VelX = stream.ReadInt16();
            //VelY = stream.ReadInt16();
            //VelZ = stream.ReadInt16();
            //var vx = stream.ReadInt16();
            //var vy = stream.ReadInt16();
            //var vz = stream.ReadInt16();
            var vel = stream.ReadVector3Short();
            Velocity = new Vector3(vel.X * 50, vel.Y * 50, vel.Z * 50);
            VelX = (short)Velocity.X;
            VelY = (short)Velocity.Y;
            VelZ = (short)Velocity.Z;

            //RotationX = (sbyte)stream.ReadInt16();
            //RotationY = (sbyte)stream.ReadInt16();
            //RotationZ = (sbyte)stream.ReadInt16();

            //var rx = (sbyte)stream.ReadInt16();
            //var ry = (sbyte)stream.ReadInt16();
            //var rz = (sbyte)stream.ReadInt16();
            //Rot = new Quaternion(Helpers.ConvertDirectionToRadian(rx), Helpers.ConvertDirectionToRadian(ry), Helpers.ConvertDirectionToRadian(rz), 1f);

            Rot = stream.ReadQuaternionShort();
            RotationX = (sbyte)Rot.X;
            RotationY = (sbyte)Rot.Y;
            RotationZ = (sbyte)Rot.Z;
        }

        public override PacketStream Write(PacketStream stream)
        {
            base.Write(stream);
            stream.WritePositionBc(X, Y, Z);
            //stream.WritePosition(Helpers.ConvertLongX(WorldPos.X), Helpers.ConvertLongX(WorldPos.Y), WorldPos.Z);

            //stream.Write(VelX);
            //stream.Write(VelY);
            //stream.Write(VelZ);
            //stream.WriteVector3Short(Velocity);
            stream.WriteVector3Short(new Vector3(Velocity.X * 0.02f, Velocity.Y * 0.02f, Velocity.Z * 0.02f));

            //stream.Write((short)RotationX);
            //stream.Write((short)RotationY);
            //stream.Write((short)RotationZ);
            stream.WriteQuaternionShort(Rot);

            return stream;
        }
    }
}
