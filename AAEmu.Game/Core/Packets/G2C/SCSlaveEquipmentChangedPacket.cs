using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Models.Game;

namespace AAEmu.Game.Core.Packets.G2C
{
    public class SCSlaveEquipmentChangedPacket : GamePacket
    {
        private readonly SlaveEquipment slaveEquipment;
        private readonly bool success;

        public SCSlaveEquipmentChangedPacket(SlaveEquipment slaveEquipment, bool success) : base(SCOffsets.SCSlaveEquipmentChangedPacket, 5)
        {

        }

        public override PacketStream Write(PacketStream stream)
        {
            // TODO coming soon!
            return stream;
        }
    }
}
