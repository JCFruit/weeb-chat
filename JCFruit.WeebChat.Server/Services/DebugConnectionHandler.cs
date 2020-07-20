using JCFruit.WeebChat.Server.Tcp;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JCFruit.WeebChat.Server.Services
{
    public class DebugConnectionHandler : IConnectionHandler
    {
        private readonly ILogger _logger;

        public DebugConnectionHandler(ILogger<DebugConnectionHandler> logger)
        {
            _logger = logger;
        }

        public Task OnClientConnected(ConnectedClient client)
        {
            //_logger.LogInformation("OnClientConnected: {clientId}", client.ClientId);
            return Task.CompletedTask;
        }

        public Task OnClientDisconnected(string clientId)
        {
            //_logger.LogInformation("OnClientDisconnected: {clientId}", clientId);
            return Task.CompletedTask;
        }

        public Task OnMessageReceived(string clientId, ReadOnlySpan<byte> data)
        {
            var message = data.Length;//Encoding.UTF8.GetString(data);
            var arr = data.ToArray();
            var errors = arr.Any(x => x == 0);
            _logger.LogInformation("OnMessageReceived: {message}. Errors = {errors}", message, errors);
            return Task.CompletedTask;
        }
    }
}
