using JCFruit.WeebChat.Server.Models;
using System;
using System.Buffers;

namespace JCFruit.WeebChat.Server.Services
{
    public class MessageSerializer
    {
        private readonly ArrayPool<byte> _pool;

        public MessageSerializer()
        {
            _pool = ArrayPool<byte>.Shared;
        }

        public T Deserialize<T>(ReadOnlySpan<byte> message)
        {
            var temp = _pool.Rent(message.Length);
            
            message.CopyTo(temp);
            
            var result = Utf8Json.JsonSerializer.Deserialize<T>(temp, Constants.PacketHeaderLength);
            
            _pool.Return(temp);
            
            return result;
        }

        public byte[] Serialize<T>(EventType type, T message)
        {
            var messageBody = Utf8Json.JsonSerializer.Serialize(message);

            var packet = new byte[messageBody.Length + Constants.PacketHeaderLength];

            BitConverter.TryWriteBytes(packet, messageBody.Length);
            BitConverter.TryWriteBytes(packet.AsSpan(Constants.EventTypeOffset), (int)type);

            Array.Copy(messageBody, 0, packet, Constants.PacketHeaderLength, messageBody.Length);

            return packet;
        }
    }
}
