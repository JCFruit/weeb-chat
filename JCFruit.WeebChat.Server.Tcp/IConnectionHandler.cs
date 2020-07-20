using System;
using System.Threading.Tasks;

namespace JCFruit.WeebChat.Server.Tcp
{
    public interface IConnectionHandler
    {
        Task OnClientConnected(ConnectedClient client);

        Task OnMessageReceived(string clientId, ReadOnlySpan<byte> data);

        Task OnClientDisconnected(string clientId);
    }
}
