using System;
using System.Collections.Generic;
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
                Title = "Hamlet",
                Author = new AuthorModel()
                {
                    Firstname = "William",
                    Lastname = "Shakespeare"
                },
                Awards = new List<AwardModel>()
                {
                    new AwardModel()
                    {
                        Name = "Award1"
                    },
                    new AwardModel()
                    {
                        Name = "Award2"
                    }
                }
            };

            bookRepository.Save(book.SerialNumber, book);
            var redisBook = bookRepository.Get(book.SerialNumber);

            var redisBookDictionary = bookRepository.HashGet(book.SerialNumber,
                x => x.Title,
                x => x.SerialNumber);

            System.Console.WriteLine(redisBookDictionary[c => c.SerialNumber]);
            System.Console.WriteLine(redisBookDictionary[c => c.Title]);
            System.Console.Read();
        }
    }
}
