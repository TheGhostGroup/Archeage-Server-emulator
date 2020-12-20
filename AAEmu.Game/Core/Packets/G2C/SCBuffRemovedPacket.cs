using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;

namespace AAEmu.Game.Core.Packets.G2C
{
    public class SCBuffRemovedPacket : GamePacket
    {
        private readonly uint _objId;
        private readonly uint _index;

        public SCBuffRemovedPacket(uint objId, uint index) : base(SCOffsets.SCBuffRemovedPacket, 5)
        {
            _objId = objId;
            _index = index;
        }

        public override PacketStream Write(PacketStream stream)
        {
            stream.WriteBc(_objId);
            stream.Write(_index);   // b - buff ? index

            return stream;
        }
    }
}
