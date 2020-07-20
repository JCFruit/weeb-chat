using JCFruit.WeebChat.Server.Tcp;
using MediatR;

namespace JCFruit.WeebChat.Server.ServerEvents
{
    public class UserConnected : INotification
    {
        public ConnectedClient Client { get; }

        public UserConnected(ConnectedClient client)
        {
            Client = client;
        }
    }
}
