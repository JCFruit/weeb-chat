using JCFruit.WeebChat.Server.ChatEvents;
using JCFruit.WeebChat.Server.Models;
using JCFruit.WeebChat.Server.ServerEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JCFruit.WeebChat.Server.Services
{
    public class ChatHandler : 
        INotificationHandler<MessageEnvelope<TextMessage>>, 
        INotificationHandler<MessageEnvelope<UserJoined>>,
        INotificationHandler<MessageEnvelope<UserLeft>>,
        INotificationHandler<MessageEnvelope<UserConnected>>
    {
        private readonly UserStorage _storage;
        private readonly MessageSerializer _serializer;
        private readonly ILogger _logger;

        public ChatHandler(UserStorage storage, MessageSerializer serializer, ILogger<ChatHandler> logger)
        {
            _storage = storage;
            _serializer = serializer;
            _logger = logger;
        }

        public async Task Handle(MessageEnvelope<TextMessage> notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("TextMessage from {userId}", notification.SourceId);

            await NotifyAllExceptSource(notification, EventType.UserSendMessage, cancellationToken);
        }

        public async Task Handle(MessageEnvelope<UserJoined> notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("User {userId} joined with name {userName}", notification.SourceId, notification.Body.Username);
            //TODO: отправить список существующих пользователей
            var user = _storage.Get(notification.SourceId);
            user.Username = notification.Body.Username;

            await NotifyAllExceptSource(notification, EventType.UserJoined, cancellationToken);
        }

        public Task Handle(MessageEnvelope<UserConnected> notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("User {userId} connected", notification.SourceId);

            var userState = new UserState(notification.Body.Client);
            _storage.Add(userState.UserId, userState);
            return Task.CompletedTask;
        }

        public async Task Handle(MessageEnvelope<UserLeft> notification, CancellationToken cancellationToken)
        {
            _logger.LogDebug("User {userId} left", notification.SourceId);

            _storage.Delete(notification.SourceId);

            await NotifyAllExceptSource(notification, EventType.UserLeft, cancellationToken);
        }

        private async Task NotifyAllExceptSource<T>(MessageEnvelope<T> notification, EventType eventType, CancellationToken cancellationToken)
        {
            var users = _storage.Get().Where(x => x.UserId != notification.SourceId);
            var messageBody = _serializer.Serialize(eventType, notification);
            await Task.WhenAll(users.Select(x => x.Client.Send(messageBody, cancellationToken)));
        }
    }
}
