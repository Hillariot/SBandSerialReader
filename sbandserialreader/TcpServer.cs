using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBandSerialReader
{
    internal class TcpServer
    {
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private readonly ConcurrentDictionary<int, TcpClient> _clients = new ConcurrentDictionary<int, TcpClient>();
        private int _clientIdCounter = 0;

        public long SentPackets => Interlocked.Read(ref _sentPackets);
        public long ReceivedPackets => Interlocked.Read(ref _receivedPackets);

        private long _sentPackets;
        private long _receivedPackets;

        public event Action<int, byte[]> DataReceived;
        public event Action<int> ClientConnected;
        public event Action<int> ClientDisconnected;

        private bool _isRunning;
        public bool IsRunning => _isRunning;

        public Task StartAsync(string ip, int port)
        {
            if (_isRunning)
                return Task.CompletedTask;

            _cts = new CancellationTokenSource();

            _listener = new TcpListener(IPAddress.Parse(ip), port);
            _listener.Start();

            _isRunning = true;

            _ = AcceptLoop(_cts.Token);

            return Task.CompletedTask;
        }

        public void Stop()
        {
            if (!_isRunning)
                return;

            _isRunning = false;

            _cts?.Cancel();

            try
            {
                _listener?.Stop();
            }
            catch { }

            foreach (var client in _clients.Values)
            {
                try { client.Close(); }
                catch { }
            }

            _clients.Clear();
        }

        private async Task AcceptLoop(CancellationToken token)
        {
            var listener = _listener;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    int clientId = Interlocked.Increment(ref _clientIdCounter);

                    _clients[clientId] = client;
                    ClientConnected?.Invoke(clientId);

                    _ = ReceiveLoop(clientId, client, token);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
            }
        }

        private async Task ReceiveLoop(int clientId, TcpClient client, CancellationToken token)
        {
            var stream = client.GetStream();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    byte[] lengthBuffer = await ReadExactAsync(stream, 4, token);
                    int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer, 0));

                    if (length <= 0 || length > 10_000_000)
                        throw new Exception("Invalid packet size");

                    byte[] payload = await ReadExactAsync(stream, length, token);

                    Interlocked.Increment(ref _receivedPackets);
                    DataReceived?.Invoke(clientId, payload);
                }
            }
            catch
            {
                // ignore
            }

            client.Close();
            _clients.TryRemove(clientId, out _);
            ClientDisconnected?.Invoke(clientId);
        }

        public async Task SendAsync(int clientId, byte[] data)
        {
            if (!_clients.TryGetValue(clientId, out var client))
                return;

            var stream = client.GetStream();

            int length = IPAddress.HostToNetworkOrder(data.Length);
            byte[] lengthBytes = BitConverter.GetBytes(length);

            await stream.WriteAsync(lengthBytes, 0, 4);
            await stream.WriteAsync(data, 0, data.Length);

            Interlocked.Increment(ref _sentPackets);
        }

        private async Task<byte[]> ReadExactAsync(NetworkStream stream, int size, CancellationToken token)
        {
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
