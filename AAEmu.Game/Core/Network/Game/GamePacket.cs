using System;
using System.Threading;
using AAEmu.Commons.Cryptography;
using AAEmu.Commons.Network;
using AAEmu.Commons.Utils;
using AAEmu.Game.Core.Network.Connections;

namespace AAEmu.Game.Core.Network.Game
{
    public abstract class GamePacket : PacketBase<GameConnection>
    {
        public byte Level { get; set; }

        protected GamePacket(ushort typeId, byte level) : base(typeId)
        {
            Level = level;
        }

        // отправляем шифрованные пакеты от сервера
        public override PacketStream Encode()
        {
            var ps = new PacketStream();
            try
            {
                var packet = new PacketStream()
                    .Write((byte)0xdd)
                    .Write(Level);

                var body = new PacketStream()
                    .Write(TypeId)
                    .Write(this);

                if (Level == 1)
                {
                    packet
                        .Write((byte)0) // hash
                        .Write((byte)0); // count
                }

                if (Level == 5)
                {
                    //пакет от сервера DD05 шифруем с помощью XOR
                    var bodyCrc = new PacketStream()
                        .Write((byte)GameConnection.EncryptSc.MNum)  // count
                        .Write(TypeId)
                        .Write(this);

                    var crc8 = GameConnection.EncryptSc.Crc8(bodyCrc); //посчитали CRC пакета

                    var data = new PacketStream();
                    data
                        .Write((byte)crc8)               // CRC
                        .Write(bodyCrc, false); // data

                    var encrypt = GameConnection.EncryptSc.StoCEncrypt(data);
                    body = new PacketStream();
                    body.Write(encrypt, false);
                    GameConnection.EncryptSc.MNum++;
                }

                packet.Write(body, false);
                
                ps.Write(packet);
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
                throw;
            }

            // SC here you can set the filter to hide packets
            if (!(TypeId == 0x013 && Level == 2) && // Pong
                !(TypeId == 0x016 && Level == 2) && // FastPong
                !(TypeId == 0x06B && Level == 1) && // SCUnitMovements
                !(TypeId == 0x06C && Level == 1))   // SCOneUnitMovement
                _log.Debug("GamePacket: S->C type {0:X3} {1}", TypeId, this.ToString().Substring(23));

            return ps;
        }

        public override PacketBase<GameConnection> Decode(PacketStream ps)
        {
            // CS here you can set the filter to hide packets
            if (!(TypeId == 0x012 && Level == 2) && // Ping
                !(TypeId == 0x015 && Level == 2) && // FastPing
                !(TypeId == 0x089 && Level == 1))   // CSMoveUnit
                _log.Debug("GamePacket: C->S type {0:X3} {1}", TypeId, this.ToString().Substring(23));

            try
            {
                Read(ps);
                //if (Level == 5)
                //{
                //    //пакет от клиента, дешифруем
                //    //------------------------------
                //    var input = new byte[ps.Count - 2];
                //    Buffer.BlockCopy(ps, 2, input, 0, ps.Count - 2);
                //    var output = DecryptCs.Decode(input, GameConnection.CryptRsa.XorKey, GameConnection.CryptRsa.AesKey, GameConnection.CryptRsa.Iv, m_numPck);
                //    m_numPck++; //увеличим номер пакета от клиента
                //    var OutBytes = new byte[output.Length + 5];
                //    Buffer.BlockCopy(ps, 0, OutBytes, 0, 5);
                //    Buffer.BlockCopy(output, 1, OutBytes, 5, output.Length - 1);
                //    //return OutBytes;
                //    ps = new PacketStream();
                //    ps.Write(OutBytes);
                //    //------------------------------
                //}
            }
            catch (Exception ex)
            {
                _log.Fatal(ex);
                throw;
            }

            return this;
        }
    }
}
