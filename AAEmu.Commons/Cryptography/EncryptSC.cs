/*
 * by NLObP: метод шифрации для ВСЕХ
*/
namespace AAEmu.Commons.Cryptography
{
    public class EncryptSc
    {
        public byte MNum { get; set; }

        /// <summary>
        /// Подсчет контрольной суммы пакета, используется в шифровании пакетов DD05 и 0005
        /// </summary>
        public byte Crc8(byte[] data, int size)
        {
            var len = size;
            uint checksum = 0;
            for (var i = 0; i <= len - 1; i++)
            {
                checksum *= 0x13;
                checksum += data[i];
            }
            return (byte)(checksum);
        }

        public byte Crc8(byte[] data)
        {
            var size = data.Length;
            return Crc8(data, size);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// вспомогательная подпрограмма для encode/decode серверных/клиентских пакетов
        /// </summary>
        /// <param name="cry"></param>
        /// <returns></returns>
        private byte Inline(ref uint cry)
        {
            cry += 0x2FCBD5U;
            var n = (byte)(cry >> 0x10);
            n = (byte)(n & 0x0F7);
            return (byte)(n == 0 ? 0x0FE : n);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// подпрограмма для encode/decode серверных пакетов, правильно шифрует и расшифровывает серверные пакеты DD05 для версии 3.0.3.0
        /// </summary>
        /// <param name="bodyPacket">адрес начиная с байта за DD05</param>
        /// <returns>возвращает адрес на подготовленные данные</returns>
        public byte[] StoCEncrypt(byte[] bodyPacket)
        {
            var length = bodyPacket.Length;
            var array = new byte[length];
            var cry = (uint)(length ^ 0x1F2175A0);
            return ByteXor(bodyPacket, length, array, cry);
        }

        private byte[] ByteXor(byte[] bodyPacket, int length, byte[] array, uint cry, int offset = 0)
        {
            var n = 4 * (length / 4);
            for (var i = n - 1 - offset; i >= 0; i--)
            {
                array[i] = (byte)(bodyPacket[i] ^ (uint)Inline(ref cry));
            }
            for (var i = n - offset; i < length; i++)
            {
                array[i] = (byte)(bodyPacket[i] ^ (uint)Inline(ref cry));
            }
            return array;
        }
    }
}
