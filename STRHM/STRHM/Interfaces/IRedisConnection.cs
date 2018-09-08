using StackExchange.Redis;

namespace STRHM.Interfaces
{
    public interface IRedisConnection
    {
        ConnectionMultiplexer GetConnection { get; }
    }
}
