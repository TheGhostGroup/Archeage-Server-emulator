using System;
using System.Collections.Generic;
using System.Numerics;

namespace AAEmu.Game.Models.Game.Transfers.Paths
{
    [Serializable]
    public class NpcsPathPoint : IComparable<NpcsPathPoint>, IComparable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public short VelX { get; set; }
        public short VelY { get; set; }
        public short VelZ { get; set; }

        public sbyte RotationX { get; set; }
        public sbyte RotationY { get; set; }
        public sbyte RotationZ { get; set; }

        public sbyte ActorDeltaMovementX { get; set; }
        public sbyte ActorDeltaMovementY { get; set; }
        public sbyte ActorDeltaMovementZ { get; set; }

        public sbyte ActorStance { get; set; }
        public sbyte ActorAlertness { get; set; }
        public ushort ActorFlags { get; set; }
        public byte Flags { get; set; }

        private const float _tolerance = 1.0f;

        public NpcsPathPoint()
        {
        }

        public NpcsPathPoint(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public NpcsPathPoint(float x, float y, float z, sbyte rotationZ)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.RotationZ = rotationZ;
        }

        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Vector2 vector2:
                    {
                        var temp = vector2;
                        return (Math.Abs(temp.X - this.X) < _tolerance && Math.Abs(temp.Y - this.Y) < _tolerance);
                    }
                case Vector3 vector3:
                    {
                        var temp = vector3;
                        return (Math.Abs(temp.X - this.X) < _tolerance && Math.Abs(temp.Y - this.Y) < _tolerance && Math.Abs(temp.Z - this.Z) < _tolerance);
                    }
                case NpcsPathPoint other:
                    return this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Z.Equals(other.Z);
                //return this.Steering.Equals(other.Steering) && this.PathPointIndex.Equals(other.PathPointIndex);

                default:
                    return false;
            }
        }

        public bool Equals(NpcsPathPoint other)
        {
            return this.X.Equals(other.X) && this.Y.Equals(other.Y) && this.Z.Equals(other.Z) && this.RotationZ == 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.X.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Y.GetHashCode();
                hashCode = (hashCode * 397) ^ this.Z.GetHashCode();
                //hashCode = (hashCode * 397) ^ this.RotationZ.GetHashCode();
                return hashCode;
                //            var hashCode = this.Steering.GetHashCode();
                //hashCode = (hashCode * 397) ^ this.PathPointIndex.GetHashCode();
                //return hashCode;
            }
        }

        //public int CompareTo(NpcPathPoint other)
        //{
        //	if (ReferenceEquals(this, other)) return 0;
        //	if (ReferenceEquals(null, other)) return 1;
        //	var steeringComparison = this.Steering.CompareTo(other.Steering);
        //	if (steeringComparison != 0) return steeringComparison;
        //	return this.PathPointIndex.CompareTo(other.PathPointIndex);
        //}

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is NpcsPathPoint other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(NpcsPathPoint)}");
        }

        public static bool operator <(NpcsPathPoint left, NpcsPathPoint right)
        {
            return Comparer<NpcsPathPoint>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(NpcsPathPoint left, NpcsPathPoint right)
        {
            return Comparer<NpcsPathPoint>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(NpcsPathPoint left, NpcsPathPoint right)
        {
            return Comparer<NpcsPathPoint>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(NpcsPathPoint left, NpcsPathPoint right)
        {
            return Comparer<NpcsPathPoint>.Default.Compare(left, right) >= 0;
        }

        public int CompareTo(NpcsPathPoint other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var xComparison = this.X.CompareTo(other.X);
            if (xComparison != 0) return xComparison;
            var yComparison = this.Y.CompareTo(other.Y);
            if (yComparison != 0) return yComparison;
            return this.Z.CompareTo(other.Z);
        }
    }
}
