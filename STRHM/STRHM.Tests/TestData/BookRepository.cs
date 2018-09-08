using STRHM.Configuration;
using STRHM.Interfaces;
using STRHM.Repositories;
using STRHM.Serialization;

namespace STRHM.Tests.TestData
{
    public class BookRepository : BaseRedisHashSetRepository<Book>
    {
        public BookRepository(IRedisConnection redisConnection, IStronglyTypedRedisSerializer serializer, RedisHashSetOptions configurationOptions) 
            : base(redisConnection, serializer, configurationOptions)
        {
        }
    }
}
