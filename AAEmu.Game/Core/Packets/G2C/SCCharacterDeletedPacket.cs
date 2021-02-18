using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;

namespace AAEmu.Game.Core.Packets.G2C
{
    public class SCCharacterDeletedPacket : GamePacket
    {
        private readonly uint _id;
        private readonly string _name;

        public SCCharacterDeletedPacket(uint id, string name) : base(SCOffsets.SCCharacterDeletedPacket, 5)
        {
            _id = id;
            _name = name;
        }

        public override PacketStream Write(PacketStream stream)
        {
            stream.Write(_id);
            stream.Write(_name);

            return stream;
        }
    }
}
