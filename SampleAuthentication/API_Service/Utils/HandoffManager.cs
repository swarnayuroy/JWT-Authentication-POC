using StackExchange.Redis;
using System.Collections.Concurrent;

namespace API_Service.Utils
{
    public interface IHandoffManager
    {
        Task<string> Create(Guid userId);
        Task<Guid?> TryConsume(string handoff);
    }

    public class HandoffManager : IHandoffManager
    {
        private readonly IDatabase _database;
        public HandoffManager(RedisConnectionTest redisConnection)
        {
            _database = redisConnection.GetDatabase();
        }        

        public async Task<string> Create(Guid userId)
        {
            var handoff = Guid.NewGuid().ToString("N");

            await _database.StringSetAsync(
                handoff,
                userId.ToString(),
                TimeSpan.FromMinutes(2));

            return handoff;
        }
        public async Task<Guid?> TryConsume(string handoff)
        {
            if (string.IsNullOrWhiteSpace(handoff))
            {
                return null;
            }

            var value = await _database.StringGetDeleteAsync(handoff);

            if (value.IsNullOrEmpty)
            {
                return null;
            }

            return Guid.TryParse(value.ToString(), out var userId)
                ? userId
                : null;
        }
    }
}
