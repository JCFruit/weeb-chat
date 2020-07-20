using JCFruit.WeebChat.Server.Tcp;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace JCFruit.WeebChat.Server.Services
{
    public class ServerHost : BackgroundService
    {
        private readonly TcpServer _server;

        public ServerHost(TcpServer server)
        {
            _server = server;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _server.Start(stoppingToken);
        }
    }
}
