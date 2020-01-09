/*
 * by uranusq https://github.com/NL0bP/aaa_emulator
 *
 * by NLObP: метод шифрации для ВСЕХ
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace AAEmu.Commons.Cryptography
{
	public class DecryptCs
	{
		public static byte[] Decode(byte[] data, uint xorKey, byte[] aesKey, byte[] iv, uint num)
		{
			//------------------------------
			//здесь распаковка пакетов от клиента 0005
			//для дешифрации следующих пакетов iv = шифрованный предыдущий пакет
			byte[] plaintext;
			byte[] ciphertext;
			var data5 = new byte[data.Length];
			var mIv = new byte[16];
			var cnt = new int[16];
			Buffer.BlockCopy(iv, 0, mIv, 0, 16); // сохраним
			//подбираем наилучшее смещение для правильной дешифровки пакета
			for (var offset = 0; offset < 16; offset++)
			{
				Buffer.BlockCopy(data, 0, data5, 0, data.Length); //получим тело пакета
				Buffer.BlockCopy(mIv, 0, iv, 0, 16); // восстановим IV
				ciphertext = DecodeXor(data5, xorKey, offset);
				plaintext = DecodeAes(ciphertext, aesKey, iv);
				cnt[offset] = CountZero(plaintext);
			}
			//окончательный вывод
			Buffer.BlockCopy(data, 0, data5, 0, data.Length); //получим тело пакета
			Buffer.BlockCopy(mIv, 0, iv, 0, 16); // восстановим IV
			var offs = IndexMaxCountZero(cnt);
			if (num == 0) { Seq = 0; }
			ciphertext = DecodeXor(data, xorKey, offs);
			plaintext = DecodeAes(ciphertext, aesKey, iv);
			return plaintext;
		}

		//--------------------------------------------------------------------------------------
		private static int CountZero(byte[] pck)
		{
			var countZero = 0;
			for (var i = 2; i < pck.Length; i++)
			{
				if (pck[i] == 0)
				{
					countZero++;
				}
			}
			return countZero;
		}
		//--------------------------------------------------------------------------------------
		private static int IndexMaxCountZero(int[] countZero)
		{
			var index = 0;
			var n = 0;
			for (var i = 0; i < countZero.Length; i++)
			{
				if (countZero[i] <= n)
				{
					continue;
				}

				n = countZero[i];
				index = i;
			}
			return index;
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
		internal static uint XorKey;
		internal static byte Seq;
		//--------------------------------------------------------------------------------------
		/// <summary>
		/// decXor packet
		/// </summary>
		/// <param name="bodyPacket">packet data, starting after message-key byte</param>
		/// <param name="msgKey">unique key for each message</param>
		/// <param name="xorKey">xor key </param>
		/// <param name="offset">xor decryption can start from some offset)</param>
		/// <returns>xor decrypted packet</returns>
		//--------------------------------------------------------------------------------------
		private static byte[] DeXor(byte[] bodyPacket, uint xorKey, uint msgKey, int offset = 0)
		{
			var length = bodyPacket.Length;
			var array = new byte[length];

			XorKey = xorKey * xorKey & 0xffffffff;
			var mul = XorKey * msgKey;

			var cry = (0x75a024a4 ^ mul) ^ 0xC3903b6a;   // 3.0.3.0 archerage.to
			var n = 4 * (length / 4);
			for (var i = n - 1 - offset; i >= 0; i--)
			{
				array[i] = (byte)(bodyPacket[i] ^ (uint)Add(ref cry));
			}

			for (var i = n - offset; i < length; i++)
			{
				array[i] = (byte)(bodyPacket[i] ^ (uint)Add(ref cry));
			}

			return array;
		}
		//--------------------------------------------------------------------------------------
		/// <summary>
		///  DecodeXor: расшифровка пакета от клиента XOR ключом
		/// </summary>
		/// <param name="bodyPacket">тело пакета начиная сразу за 0005</param>
		/// <param name="xorKey"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------------
		public static byte[] DecodeXor(byte[] bodyPacket, uint xorKey, int offset)
		{
			var map = new Dictionary<int, int>
			{
				{0x30, 0x01}, {0x31, 0x02}, {0x32, 0x03}, {0x33, 0x04}, {0x34, 0x05}, {0x35, 0x06}, {0x36, 0x07}, {0x37, 0x08},
				{0x38, 0x09}, {0x39, 0x0a}, {0x3a, 0x0b}, {0x3b, 0x0c}, {0x3c, 0x0d}, {0x3d, 0x0e}, {0x3e, 0x0f}, {0x3f, 0x10}
			};
			var length = bodyPacket.Length;
			var mBodyPacket = new byte[length - 3];
			Buffer.BlockCopy(bodyPacket, 3, mBodyPacket, 0, length - 3);
			var packet = new byte[mBodyPacket.Length];
			var msgKey = (uint)(bodyPacket.Length / 16 - 1) << 4;
			msgKey += (uint)map[bodyPacket[2]];
			packet = DeXor(mBodyPacket, xorKey, msgKey, offset);
			return packet;
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
