using JCFruit.WeebChat.Server.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace JCFruit.WeebChat.Server.Services
{
    public class UserStorage
    {
        private readonly ConcurrentDictionary<string, UserState> _storage;

        public UserStorage()
        {
            _storage = new ConcurrentDictionary<string, UserState>();
        }

        public UserState Get(string userId)
        {
            _storage.TryGetValue(userId, out var user);
            return user;
        }

        public IEnumerable<UserState> Get()
        {
            return _storage.Values.ToArray();
        }

        public void Add(string userId, UserState user)
        {
            _storage.AddOrUpdate(userId, user, (k, v) => user);
        }

        public UserState Delete(string userId)
        {
            _storage.TryRemove(userId, out var user);
            return user;
        }
    }
}
