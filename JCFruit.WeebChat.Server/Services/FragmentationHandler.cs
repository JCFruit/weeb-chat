using JCFruit.WeebChat.Server.Tcp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JCFruit.WeebChat.Server.Services
{
    public class FragmentationHandler : IConnectionHandler
    {
        private readonly IConnectionHandler _implementation;
        private readonly Dictionary<string, FragmentedMessage> _storage;

        public FragmentationHandler(IConnectionHandler implementation)
        {
            _implementation = implementation;
            _storage = new Dictionary<string, FragmentedMessage>();
        }

        public Task OnClientConnected(ConnectedClient client)
        {
            return _implementation.OnClientConnected(client);
        }

        public Task OnClientDisconnected(string clientId)
        {
            if (_storage.ContainsKey(clientId))
                _storage.Remove(clientId);
            return _implementation.OnClientDisconnected(clientId);
        }

        public Task OnMessageReceived(string clientId, ReadOnlySpan<byte> data)
        {
            if(!_storage.TryGetValue(clientId, out var fragmentedMessage)) // Получали ли мы уже кусок сообщения
            {
                var length = BitConverter.ToInt32(data.Slice(0, 4));
                if(length + 8 == data.Length) // Сообщение полностью уместилось в пакет
                {
                    return _implementation.OnMessageReceived(clientId, data);
                }
                else
                {
                    fragmentedMessage = new FragmentedMessage
                    {
                        Length = length + 8,
                        CurrentLength = data.Length,
                        Data = new byte[length + 8]

                    };
                    data.CopyTo(fragmentedMessage.Data);
                    _storage.Add(clientId, fragmentedMessage);
                    return Task.CompletedTask;
                }
            }
            else
            {
                var dataArray = data.ToArray();

                if (fragmentedMessage.CurrentLength + data.Length >= fragmentedMessage.Length) // Сообщение - оставшаяся часть прошлого сообщения
                {
                    _storage.Remove(clientId);

                    var trailerLength = fragmentedMessage.Length - fragmentedMessage.CurrentLength;
                    
                    Array.Copy(dataArray, 0, fragmentedMessage.Data, fragmentedMessage.CurrentLength, trailerLength);

                    return OnMessageReceivedWithContinuation(clientId, fragmentedMessage.Data, dataArray, trailerLength);
                }
                else // Мы получили еще часть сообщения, но это еще не все сообщение
                {
                    Array.Copy(dataArray, 0, fragmentedMessage.Data, fragmentedMessage.CurrentLength, dataArray.Length);
                    return Task.CompletedTask;
                }
            }
        }

        private async Task OnMessageReceivedWithContinuation(string clientId, byte[] fullMessage, byte[] reminder, int reminderStartIndex)
        {
            await _implementation.OnMessageReceived(clientId, fullMessage);
            if(reminderStartIndex < reminder.Length)
                await OnMessageReceived(clientId, reminder.AsSpan(reminderStartIndex)); // рекурсивно зовем метод на случай, если оставшаяся часть тоже не полная
        }

        private class FragmentedMessage
        {
            public byte[] Data { get; set; }

            public int Length { get; set; }

            public int CurrentLength { get; set; }
        }
    }
}
