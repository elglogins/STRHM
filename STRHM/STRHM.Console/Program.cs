using System;
using System.Collections.Generic;
using STRHM.Collections;
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

            bookRepository.HashSet(book.SerialNumber, new StronglyTypedDictionary<BookModel>
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

            var redisBookDictionary = bookRepository.HashGet(book.SerialNumber,
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
