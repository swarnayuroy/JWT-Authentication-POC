using System.Collections.Concurrent;

namespace API_Service.Utils
{
    public class HandoffData
    {
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class HandoffManager
    {
        private static readonly ConcurrentDictionary<string, HandoffData> _handoffs = new();

        public string Create(Guid userId)
        {
            var handoff = Guid.NewGuid().ToString("N");

            _handoffs[handoff] = new HandoffData
            {
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(2)
            };

            return handoff;
        }
        public bool TryConsume(string handoff, out Guid userId)
        {
            userId = Guid.Empty;

            if (!_handoffs.TryRemove(handoff, out var data))
            {
                return false;
            }

            if (data.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            userId = data.UserId;
            return true;
        }
    }
}
