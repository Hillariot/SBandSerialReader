using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBandSerialReader
{
    internal class PacketProtocol
    {
        public static async Task WritePacketAsync(NetworkStream stream, byte[] data, CancellationToken token = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (data == null) throw new ArgumentNullException(nameof(data));

            int length = IPAddress.HostToNetworkOrder(data.Length);
            byte[] lengthBytes = BitConverter.GetBytes(length);

            await stream.WriteAsync(lengthBytes, 0, 4, token);
            await stream.WriteAsync(data, 0, data.Length, token);
            await stream.FlushAsync(token);
        }

        public static async Task<byte[]> ReadPacketAsync(NetworkStream stream, CancellationToken token = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            byte[] lengthBuffer = await ReadExactAsync(stream, 4, token);
            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

            if (length <= 0 || length > 10_000_000)
                throw new Exception("Invalid packet size");

            return await ReadExactAsync(stream, length, token);
        }

        public static async Task<byte[]> ReadExactAsync(NetworkStream stream, int size, CancellationToken token = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));

            byte[] buffer = new byte[size];
            int read = 0;

            while (read < size)
            {
                int r = await stream.ReadAsync(buffer, read, size - read, token);
                if (r == 0)
                    throw new Exception("Disconnected");

                read += r;
            }

            return buffer;
        }
    }
}
