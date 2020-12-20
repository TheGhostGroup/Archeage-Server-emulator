using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.CashShop;

namespace AAEmu.Game.Core.Packets.C2G
{
    public class CSPremiumServiceListPacket : GamePacket
    {
        public CSPremiumServiceListPacket() : base(CSOffsets.CSPremiumServiceListPacket, 5)
        {
        }

        public override void Read(PacketStream stream)
        {
            // Empty struct

            _log.Warn("PremiumServiceList");

            var detail = new PremiumDetail
            {
                CId = 8000001,
                CName = "Премиум-подпиcка (30 дней)",
                PId = 1,
                PTime = 720,
                Price = 300
            };

            Connection.SendPacket(new SCPremiumServiceListPacket(true, 1, detail, 0));
        }
    }
}
