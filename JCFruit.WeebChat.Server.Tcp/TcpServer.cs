using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JCFruit.WeebChat.Server.Tcp
{
    public class TcpServer
    {
        private readonly ArrayPool<byte> _bufferPool;
        private readonly TcpListener _listener;
        private readonly IConnectionHandler _handler;
        private readonly ILogger _logger;

        public TcpServer(TcpOptions options, IConnectionHandler handler, ILogger<TcpServer> logger)
        {
            _bufferPool = ArrayPool<byte>.Shared;
            _listener = new TcpListener(options.IPAddress, options.Port);
            _handler = handler;
            _logger = logger;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Listening on {address}", _listener.LocalEndpoint);
            _listener.Start();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    var task = OnClientConnected(client, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in connection loop");
                throw;
            }
            finally
            {
                _logger.LogInformation("Stop listening on {address}", _listener.Server.LocalEndPoint);
                _listener.Stop();
            }
        }

        private async Task OnClientConnected(TcpClient client, CancellationToken cancellationToken)
        {
            var clientId = Guid.NewGuid().ToString();
            var address = client.Client.RemoteEndPoint;
            _logger.LogDebug("Accepted Client[{clientId}] at {address}", clientId, address);

            try
            {
                using (client)
                {
                    var stream = client.GetStream();

                    var connectedClient = new ConnectedClient(clientId, stream);

                    await _handler.OnClientConnected(connectedClient);

                    while (!cancellationToken.IsCancellationRequested && client.Connected)
                    {
                        var buffer = _bufferPool.Rent(client.ReceiveBufferSize);

                        var datagramLength = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        if (datagramLength == 0)
                        {
                            _logger.LogDebug("Client[{clientId}] disconnected", clientId);
                            break;
                        }

                        await _handler.OnMessageReceived(clientId, buffer.AsSpan(0, datagramLength));

                        _bufferPool.Return(buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    _logger.LogInformation("Client[{clientId}] forcefully closed connection", clientId);
                }
                else
                {
                    _logger.LogError(ex, "Error handling messages from Client[{clientId}]", clientId);
                }
            }
            finally
            {
                await _handler.OnClientDisconnected(clientId);
                _logger.LogDebug("Client[{clientId}] at {address} disconnected", clientId, address);
            }
        }
    }
}
