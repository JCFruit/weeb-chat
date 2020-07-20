using MediatR;
using System;

namespace JCFruit.WeebChat.Server.Models
{
    public class MessageEnvelope<T> : INotification
    {
        public string SourceId { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public T Body { get; set; }
    }
}
