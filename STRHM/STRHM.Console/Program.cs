using System;
using STRHM.Console.Models;
using STRHM.Console.Repositories;

namespace STRHM.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var bookRepository = new BookRepository();
            var book = new BookModel()
            {
                Featured = true,
                PublishedOn = new DateTime(2003, 12, 12),
                Rating = 8,
                SerialNumber = "72084354",
                Title = "Hamlet"
            };

            bookRepository.Save(book.SerialNumber, book);
            var redisBook = bookRepository.Get(book.SerialNumber);
        }
    }
}
