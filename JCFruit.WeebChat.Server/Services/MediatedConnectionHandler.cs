using JCFruit.WeebChat.Server.ChatEvents;
using JCFruit.WeebChat.Server.Models;
using JCFruit.WeebChat.Server.ServerEvents;
using JCFruit.WeebChat.Server.Tcp;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JCFruit.WeebChat.Server.Services
{
    public class MediatedConnectionHandler : IConnectionHandler
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        private readonly MessageSerializer _serializer;

        public MediatedConnectionHandler(IMediator mediator, MessageSerializer serializer, ILogger<MediatedConnectionHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
            _serializer = serializer;
        }

        public async Task OnClientConnected(ConnectedClient client)
        {
            await _mediator.Publish(new MessageEnvelope<UserConnected>
            {
                SourceId = client.ClientId,
                Timestamp = DateTimeOffset.UtcNow,
                Body = new UserConnected(client)
            });
        }

        public Task OnClientDisconnected(string clientId)
        {
            return PublishMessage(clientId, new UserLeft(LeftReason.Disconnected));
        }

        public Task OnMessageReceived(string clientId, ReadOnlySpan<byte> data)
        {
            var messageType = (EventType)BitConverter.ToInt32(data.Slice(Constants.EventTypeOffset));

            _logger.LogDebug("Client[{clientId}] send {messageType} message", clientId, messageType);
            
            switch (messageType)
            {
                case EventType.UserJoined:
                    var joinMessage = _serializer.Deserialize<UserJoined>(data);
                    return PublishMessage(clientId, joinMessage);
                case EventType.UserLeft:
                    var leftMessage = new UserLeft(LeftReason.Voluntarily);
                    return PublishMessage(clientId, leftMessage);
                case EventType.UserSendMessage:
                    var textMessage = _serializer.Deserialize<TextMessage>(data);
                    return PublishMessage(clientId, textMessage);
            }

            _logger.LogWarning("Unknown EventType: {messageType}", messageType);
            return Task.CompletedTask;
        }

        private async Task PublishMessage<T>(string clientId, T message)
        {
            await _mediator.Publish(new MessageEnvelope<T>
            {
                SourceId = clientId,
                Timestamp = DateTimeOffset.UtcNow,
                Body = message
            });
        }
    }
}
