using JCFruit.WeebChat.Server.ChatEvents;
using JCFruit.WeebChat.Server.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JCFruit.WeebChat.Server.Services
{
    public class TextMessageHandler : NotificationHandler<MessageEnvelope<TextMessage>>
    {
        private readonly ILogger _logger;

        public TextMessageHandler(ILogger<TextMessageHandler> logger)
        {
            _logger = logger;
        }

        //public Task Handle(MessageEnvelope<TextMessage> notification, CancellationToken cancellationToken)
        //{
            
        //    return Task.CompletedTask;
        //}

        protected override void Handle(MessageEnvelope<TextMessage> notification)
        {
            _logger.LogInformation("SourceId: {sourceId}. Text: {text}", notification.SourceId, notification.Body.Text);
        }
    }
}
