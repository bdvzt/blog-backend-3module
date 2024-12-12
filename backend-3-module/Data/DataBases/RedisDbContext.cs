using StackExchange.Redis;

namespace backend_3_module.Data.DataBases;

public class RedisDbContext
{
    private readonly IDatabase _database;

    public RedisDbContext(string? connectionString)
    {
        if (connectionString != null)
        {
            var connection = ConnectionMultiplexer.Connect(connectionString);
            _database = connection.GetDatabase();
        }
    }
    
    public async Task AddTokenToBlackList(string? token)
    {
        if (token != null) 
            await _database.StringSetAsync(token, "blackList", TimeSpan.FromMinutes(60));
    }

    public async Task<bool> IsBlacklisted(string token)
    {
        return await _database.KeyExistsAsync(token);
    }
}