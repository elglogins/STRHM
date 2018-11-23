using StackExchange.Redis;
using STRHM.Configuration;
using STRHM.Interfaces;
using STRHM.Repositories;

namespace STRHM.Tests.TestData
{
    public class BookRepository : BaseRedisHashSetRepository<Book>
    {
        private readonly RedisConnection _redisConnection;

        public BookRepository(RedisConnection redisConnection, IStronglyTypedRedisSerializer serializer, RedisHashSetOptions configurationOptions)
            : base(serializer, configurationOptions)
        {
            _redisConnection = redisConnection;
        }

        protected override IDatabase GetDatabase(int databaseId)
        {
            return _redisConnection.Connection.GetDatabase(databaseId);
        }
    }
}
