/*
 * by uranusq https://github.com/NL0bP/aaa_emulator
 *
 * by NLObP: оригинальный метод шифрации (как в crynetwork.dll)
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace AAEmu.Commons.Cryptography
{
	public class DecryptCs
	{
		//--------------------------------------------------------------------------------------
		public static byte[] Decode(byte[] data, uint xorKey, byte[] aesKey, byte[] iv, uint num)
		{
			//------------------------------
			// здесь распаковка пакетов от клиента 0005
			// для дешифрации следующих пакетов iv = шифрованный предыдущий пакет
			byte[] plaintext;
			byte[] ciphertext;
			if (num == 0)
			{
				Seq = 0;
				seq = 0;
			}
			ciphertext = DecodeXor(data, xorKey);
			plaintext = DecodeAes(ciphertext, aesKey, iv);
			return plaintext;
		}
        //--------------------------------------------------------------------------------------
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
        ///  toClientEncr help function
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static byte Add(ref uint cry)
		{
			cry += 0x2FCBD5;
			var n = (byte)(cry >> 0x10);
			n = (byte)(n & 0x0F7);
			return (byte)(n == 0 ? 0x0FE : n);
		}
		//--------------------------------------------------------------------------------------
		/// <summary>
		///  toClientEncr help function
		/// </summary>
		/// <returns></returns>
		private static byte MakeSeq(ref uint seq)
		{
			seq += 0x2FA245;
			byte result = (byte)(seq >> 0xE & 0x73);
			if (result == 0)
				result = (byte)0xFEu;
			return result;
		}
		//--------------------------------------------------------------------------------------
		internal static uint XorKey;
		internal static byte Seq;
		internal static uint seq;
		//--------------------------------------------------------------------------------------
		public static byte[] DecodeXor(byte[] bodyPacket, uint xorKey)
		{
			//          +-Hash начало блока для DecodeXOR, где второе число, в данном случае F(16 байт)-реальная длина данных в пакете, к примеру A(10 байт)-реальная длина данных в пакете
			//          |  +-начало блока для DecodeAES
			//          V  V
			//1300 0005 3F D831012E6DFA489A268BC6AD5BC69263
			var map = new Dictionary<int, int>
			{
				{0x30, 0x1}, {0x31, 0x2}, {0x32, 0x3}, {0x33, 0x4}, {0x34, 0x5}, {0x35, 0x6}, {0x36, 0x7}, {0x37, 0x8},
				{0x38, 0x9}, {0x39, 0xa}, {0x3a, 0xb}, {0x3b, 0xc}, {0x3c, 0xd}, {0x3d, 0xe}, {0x3e, 0xf}, {0x3f, 0x10}
			};
			var mBodyPacket = new byte[bodyPacket.Length - 3];
			Buffer.BlockCopy(bodyPacket, 3, mBodyPacket, 0, bodyPacket.Length - 3);
			var msgKey = (uint)(bodyPacket.Length / 16 - 1) << 4;
			msgKey += (uint)map[bodyPacket[2]]; // это реальная длина данных в пакете
			var array = new byte[mBodyPacket.Length];
			XorKey = xorKey * xorKey & 0xffffffff;
			var mul = msgKey * XorKey;
			var cry = mul ^ ((uint)MakeSeq(ref seq) + 0x75a024a4) ^ 0xc3903b6a; // 3.0.3.0 archerage.to
			var offset = 4;
			if (Seq != 0)
			{
				if (Seq % 3 != 0)
				{
					if (Seq % 5 != 0)
					{
						if (Seq % 7 != 0)
						{
							if (Seq % 9 != 0)
							{
								if (!(Seq % 11 != 0)) { offset = 7; }
							}
							else { offset = 3; }
						}
						else { offset = 11; }
					}
					else { offset = 2; }
				}
				else { offset = 5; }
			}
			else { offset = 9; }
			var n = offset * (mBodyPacket.Length / offset);
			for (var i = n - 1; i >= 0; i--)
			{
				array[i] = (byte)(mBodyPacket[i] ^ (uint)Add(ref cry));
			}
			for (var i = n; i < mBodyPacket.Length; i++)
			{
				array[i] = (byte)(mBodyPacket[i] ^ (uint)Add(ref cry));
			}
			Seq += MakeSeq(ref seq);
			Seq += 1;

			return array;
		}
		//--------------------------------------------------------------------------------------
		private const int Size = 16;
		//--------------------------------------------------------------------------------------
		private static RijndaelManaged CryptAes(byte[] aesKey, byte[] iv)
		{
			var rm = new RijndaelManaged
			{
				KeySize = 128,
				BlockSize = 128,
				Padding = PaddingMode.None,
				Mode = CipherMode.CBC,
				Key = aesKey,
				IV = iv
			};
			return rm;
		}
		//--------------------------------------------------------------------------------------
		/// <summary>
		/// DecodeAes: расшифровка пакета от клиента AES ключом
		/// </summary>
		/// <param name="cipherData"></param>
		/// <param name="aesKey"></param>
		/// <param name="iv"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------------
		public static byte[] DecodeAes(byte[] cipherData, byte[] aesKey, byte[] iv)
		{
			var mIv = new byte[16];
			Buffer.BlockCopy(iv, 0, mIv, 0, Size);
			var len = cipherData.Length / Size;
			//Save last 16 bytes in IV
			Buffer.BlockCopy(cipherData, (len - 1) * Size, iv, 0, Size);
			// Create a MemoryStream that is going to accept the decrypted bytes
			using (var memoryStream = new MemoryStream())
			{
				// Create a symmetric algorithm.
				// We are going to use RijndaelRijndael because it is strong and available on all platforms.
				// You can use other algorithms, to do so substitute the next line with something like
				// TripleDES alg = TripleDES.Create();
				using (var alg = CryptAes(aesKey, mIv))
				{
					// Create a CryptoStream through which we are going to be pumping our data.
					// CryptoStreamMode.Write means that we are going to be writing data to the stream
					// and the output will be written in the MemoryStream we have provided.
					using (var cs = new CryptoStream(memoryStream, alg.CreateDecryptor(), CryptoStreamMode.Write))
					{
						// Write the data and make it do the decryption
						cs.Write(cipherData, 0, cipherData.Length);

						// Close the crypto stream (or do FlushFinalBlock).
						// This will tell it that we have done our decryption and there is no more data coming in,
						// and it is now a good time to remove the padding and finalize the decryption process.
						cs.FlushFinalBlock();
						cs.Close();
					}
				}
				// Now get the decrypted data from the MemoryStream.
				// Some people make a mistake of using GetBuffer() here, which is not the right way.
				var decryptedData = memoryStream.ToArray();
				return decryptedData;
			}
		}
	}
}
