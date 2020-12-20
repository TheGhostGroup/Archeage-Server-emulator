using System;

using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;

namespace AAEmu.Game.Core.Packets.Proxy
{
    public class PongPacket : GamePacket
    {
        private readonly long _tm;
        private readonly long _when;
        private readonly long _remote;
        private readonly uint _local;
        private readonly uint _world;

        public PongPacket(long tm, long when, uint local) : base(0x013, 2)
        {
            _tm = tm;
            _when = when;
            _local = local;
            _world = (uint)(Environment.TickCount & int.MaxValue);
            _remote = _world * 1000;
        }

        public override PacketStream Write(PacketStream stream)
        {
            stream.Write(_tm);
            stream.Write(_when);
            stream.Write((long)0); // elapsed
            stream.Write(_remote); // world * 1000; remote
            stream.Write(_local);
            stream.Write(_world); // TODO packet sleep 250ms...

            return stream;
        }
    }
}