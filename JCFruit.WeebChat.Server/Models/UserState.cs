using JCFruit.WeebChat.Server.Tcp;

namespace JCFruit.WeebChat.Server.Models
{
    public class UserState
    {
        public ConnectedClient Client { get; }

        public string UserId => Client.ClientId;

        public string Username { get; set; }

        public UserState(ConnectedClient client)
        {
            Client = client;
        }
    }
}
