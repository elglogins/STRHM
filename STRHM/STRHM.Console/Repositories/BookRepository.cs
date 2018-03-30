using STRHM.Console.Models;
using STRHM.Repositories;

namespace STRHM.Console.Repositories
{
    class BookRepository : BaseRedisHashSetRepository<BookModel>
    {
        public BookRepository() : base("books:", 0)
        {
        }
    }
}
