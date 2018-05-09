using STRHM.Console.Models;
using STRHM.Repositories;
using System.Configuration;

namespace STRHM.Console.Repositories
{
    class BookRepository : BaseRedisHashSetRepository<BookModel>
    {
        public BookRepository() : base(ConfigurationManager.ConnectionStrings["RedisConnectionString"].ConnectionString, "books:", 0)
        {
        }
    }
}
