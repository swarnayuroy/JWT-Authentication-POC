using Azure.Identity;
using StackExchange.Redis;

namespace API_Service.Utils
{
    public class RedisConnectionTest
    {
        private readonly ConnectionMultiplexer _connection;

        private RedisConnectionTest(ConnectionMultiplexer connection)
        {
            _connection = connection;
        }
        public IDatabase GetDatabase()
        {
            return _connection.GetDatabase();
        }

        public static async Task<RedisConnectionTest> CreateAsync(IConfiguration configuration)
        {
            try
            {
                var endpoint = configuration["Redis:Endpoint"];

                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    throw new InvalidOperationException(
                        "Redis endpoint is not configured.");
                }
                var options = ConfigurationOptions.Parse(endpoint);

                options.Protocol = RedisProtocol.Resp3;
                options.AbortOnConnectFail = false;

                await options.ConfigureForAzureWithTokenCredentialAsync(
                    new DefaultAzureCredential());

                var connection = await ConnectionMultiplexer.ConnectAsync(options);

                await connection.GetDatabase().PingAsync();

                Console.WriteLine("Connected to Azure Redis successfully.");

                return new RedisConnectionTest(connection);
            }
            catch (Exception)
            {
                Console.WriteLine("Falling back to local redis...");
                var endpoint = configuration["Redis:LocalEndpoint"];
                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    throw new InvalidOperationException(
                        "Local Redis endpoint is not configured.");
                }
                ConnectionMultiplexer? dockerConnection = null;

                try
                {
                    var dockerOptions = ConfigurationOptions.Parse(endpoint);
                    dockerOptions.Protocol = RedisProtocol.Resp3;
                    dockerOptions.AbortOnConnectFail = false;

                    dockerConnection = await ConnectionMultiplexer.ConnectAsync(dockerOptions);

                    await dockerConnection.GetDatabase().PingAsync();

                    Console.WriteLine("Connected to Docker Redis successfully.");

                    return new RedisConnectionTest(dockerConnection);
                }
                catch (Exception dockerException)
                {
                    if (dockerConnection != null)
                    {
                        await dockerConnection.DisposeAsync();
                    }

                    throw new InvalidOperationException(
                        "Both Azure Redis and local Redis connections failed.",
                        dockerException);
                }                
            }            
        }

        public async Task<bool> PingAsync()
        {
            var result = await GetDatabase().PingAsync();

            return result >= TimeSpan.Zero;
        }

        public async Task<bool> SetGetDeleteAsync()
        {
            var database = GetDatabase();

            var key = $"test:{Guid.NewGuid():N}";
            var value = Guid.NewGuid().ToString();

            // SET
            await database.StringSetAsync(
                key,
                value,
                TimeSpan.FromMinutes(2));

            // GET
            var retrievedValue = await database.StringGetAsync(key);

            // DELETE
            await database.KeyDeleteAsync(key);

            return retrievedValue == value;
        }
    }
}
