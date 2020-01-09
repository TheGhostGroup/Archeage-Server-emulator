using System.IO;
using System.IO.Compression;

namespace AAEmu.Commons.Compression
{
	public static class Сompressing
	{
		/// <summary>
		/// Decompress DD04 packets
		/// author: Atelo
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static Stream DecompressA(byte[] data)
		{
			var output = new MemoryStream();
			using (var compressedStream = new MemoryStream(data))
			using (var zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
			{
				zipStream.CopyTo(output);
				zipStream.Close();
				output.Position = 0;
				return output;
			}
		}
		/// <summary>
		/// Decompress DD04 packets
		/// author: Atelo
		/// </summary>
		public static byte[] ReadFullyA(Stream input)
		{
			var buffer = new byte[16 * 1024];
			using (var ms = new MemoryStream())
			{
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				return ms.ToArray();
			}
		}
		//======================================================================================================
		//Функции для работы со сжатием строк
		/// <summary>
		/// Упаковка(архивирование) строки.
		/// На входе — данные. На выходе — сжатая строка.
		/// Compress DD04 и DD03 packets
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Compress(byte[] data)
		{
			var output = new MemoryStream();
			using (var dstream = new DeflateStream(output, CompressionMode.Compress))
			{
				dstream.Write(data, 0, data.Length);
				dstream.Close();
			}

			return output.ToArray();
		}
		/// <summary>
		/// Распаковка(разархивирование) сжатой строки.
		/// На входе — данные, предварительно сжатые предыдущей функцией. На выходе — распакованная строка.
		/// Decompress DD04 packets. На вход подавать пакет без длины, начиная с DD04
		/// Decompress DD03 packets. На вход подавать пакет без длины и DD, начиная с 03
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Decompress(byte[] data)
		{
			var input = new MemoryStream(data);
			var output = new MemoryStream();
			using (var dstream = new DeflateStream(input, CompressionMode.Decompress))
			{
				dstream.CopyTo(output);
				dstream.Close();
			}

			return output.ToArray();
		}
	}
}

