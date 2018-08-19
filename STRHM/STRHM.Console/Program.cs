using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using STRHM.Collections;
using STRHM.Console.Models;
using STRHM.Console.Repos;

namespace STRHM.Console
{
    class Program
    {
        static void Main()
        {
            MainAsync().Wait();
            // or, if you want to avoid exceptions being wrapped into AggregateException:
            //  MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
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

            await bookRepository.SaveAsync(book.SerialNumber, book);
            await bookRepository.SetExpirationAsync(book.SerialNumber, TimeSpan.FromMinutes(2));
            await bookRepository.RemoveExpirationAsync(book.SerialNumber);
            var redisBook = await bookRepository.GetAsync(book.SerialNumber);

            await bookRepository.HashSetAsync(book.SerialNumber, new StronglyTypedDictionary<BookModel>
            {
                {x=>x.Rating, 10},
                {x=>x.PublishedOn, DateTime.Now},
                {x=>x.Author, new AuthorModel()
                    {
                        Firstname = "John",
                        Lastname = "Smith"
                    }
                }
            });

            await  bookRepository.SetExpirationAsync(book.SerialNumber, TimeSpan.FromSeconds(5));

            var redisBookDictionary = await bookRepository.HashGetAsync(book.SerialNumber, CommandFlags.None, 
                x => x.Title,
                x => x.SerialNumber,
                x => x.Rating,
                x => x.Author);

            var rating = redisBookDictionary.Get<int>(x => x.Rating);
            var serialNumber = redisBookDictionary.Get<int>(x => x.SerialNumber);
            var author = redisBookDictionary.Get<AuthorModel>(x => x.Author);
        }
    }
}
