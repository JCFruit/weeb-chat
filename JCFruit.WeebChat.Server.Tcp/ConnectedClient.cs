using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JCFruit.WeebChat.Server.Tcp
{
    public class ConnectedClient
    {
        private readonly NetworkStream _stream;

        public string ClientId { get; }

        public ConnectedClient(string clientId, NetworkStream stream)
        {
            ClientId = clientId;
            _stream = stream;
        }

        public async Task Send(byte[] data, CancellationToken cancellationToken = default)
        {
            await _stream.WriteAsync(data, 0, data.Length, cancellationToken);
        }

        public void Close()
        {
            _stream.Close();
        }
    }
}
