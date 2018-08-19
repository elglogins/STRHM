using STRHM.Console.Models;
using STRHM.Repositories;

namespace STRHM.Console.Repos
{
    class BookRepository : BaseRedisHashSetRepository<BookModel>
    {
        public BookRepository() 
            : base("localhost:6379", "book:", 0)
        {
        }
    }
}
