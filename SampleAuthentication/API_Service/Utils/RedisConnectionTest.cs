using Azure.Identity;
using StackExchange.Redis;

namespace API_Service.Utils
{
    public class RedisConnectionTest: IAsyncDisposable
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
            ConnectionMultiplexer? azureConnection = null;
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

                await options.ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential());

                azureConnection = await ConnectionMultiplexer.ConnectAsync(options);

                await azureConnection.GetDatabase().PingAsync();

                Console.WriteLine("Connected to Azure Redis successfully.");

                return new RedisConnectionTest(azureConnection);
            }
            catch (Exception)
            {
                if (azureConnection != null)
                {
                    await azureConnection.DisposeAsync();
                }
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

        public ValueTask DisposeAsync()
        {
            return _connection.DisposeAsync();
        }
    }
}
