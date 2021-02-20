using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Core.Packets.G2C
{
    public class SCTransferTelescopeUnitsPacket : GamePacket
    {
        private readonly bool _last;
        private readonly Transfer[] _transfers;

        public SCTransferTelescopeUnitsPacket(bool last, Transfer[] transfers) : base(SCOffsets.SCTransferTelescopeUnitsPacket, 5)
        {
            _last = last;
            _transfers = transfers;

            /*
               // вызов пакета SCTransferTelescopeUnitsPacket
               var transfers = TransferManager.Instance.GetTransfers();
               if (transfers.Length == 0)
               {
                    Connection.SendPacket(new SCTransferTelescopeUnitsPacket(true, transfers));
               }
               else
               {
                    for (var i = 0; i < transfers.Length; i += 2)
                    {
                        var last = transfers.Length - i <= 2;
                        var temp = new Transfer[last ? transfers.Length - i : 2];
                        Array.Copy(transfers, i, temp, 0, temp.Length);
                        Connection.SendPacket(new SCTransferTelescopeUnitsPacket(last, temp));
                    }
               }               
             */
        }

        public override PacketStream Write(PacketStream stream)
        {
            stream.Write(_last);
            stream.Write((byte)_transfers.Length);
            foreach (var transfer in _transfers)
            {
                transfer.Write(stream);
            }

            return stream;
        }
    }
}
