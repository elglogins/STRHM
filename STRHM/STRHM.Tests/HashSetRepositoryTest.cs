using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using STRHM.Collections;
using STRHM.Configuration;
using STRHM.Repositories;
using STRHM.Serialization.NewtonsoftJson;
using STRHM.Tests.TestData;
using Xunit;

namespace STRHM.Tests
{
    public class HashSetRepositoryTest
    {
        private readonly BookRepository _bookRepository;

        public HashSetRepositoryTest()
        {
            _bookRepository = new BookRepository(
                new RedisConnection(new ConfigurationOptions()
                {
                    EndPoints = { "localhost:6379" }
                }), 
                new StronglyTypedRedisNewtonsoftSerializer(),  
                new RedisHashSetOptions()
                    {
                        Database = 0,
                        KeyNamespace = "books:"
                    }
                );
        }

        [Fact]
        public async void SetAndGetHashObjectValues()
        {
            var book = new Book()
            {
                Id = 3,
                PublishedOn = DateTime.Now,
                Authors = new List<Author>()
                {
                    new Author()
                    {
                        Firstname = "John",
                        Lastname = "Smith"
                    }
                },
                Rating = null
            };

            await _bookRepository.SaveAsync(book.Id.ToString(), book);
            var savedBook = await _bookRepository.HashGetAsync(book.Id.ToString(),
                b => b.Id,
                b => b.PublishedOn,
                b => b.Authors,
                b => b.Rating
                );

            Assert.NotNull(savedBook);

            Assert.False(savedBook.HasValue(b => b.Rating));
            Assert.True(savedBook.HasValue(b => b.Id));
            Assert.True(savedBook.HasValue(b => b.PublishedOn));
            Assert.True(savedBook.HasValue(b => b.Authors));

            Assert.Equal(book.PublishedOn.ToShortDateString(), savedBook.Get<DateTime>(b=>b.PublishedOn).ToShortDateString());
            Assert.Equal(book.PublishedOn.ToShortTimeString(), savedBook.Get<DateTime>(b=>b.PublishedOn).ToShortTimeString());
            Assert.Equal(book.Authors.Count, savedBook.Get<IEnumerable<Author>>(b=> b.Authors).Count());

            Assert.Equal(book.Authors.FirstOrDefault()?.Firstname, savedBook.Get<IEnumerable<Author>>(b => b.Authors).FirstOrDefault()?.Firstname);
            Assert.Equal(book.Authors.FirstOrDefault()?.Lastname, savedBook.Get<IEnumerable<Author>>(b => b.Authors).FirstOrDefault()?.Lastname);

            Assert.Equal(book.Rating, savedBook.Get<int?>(b => b.Rating));

            var deletionResult = await _bookRepository.DeleteAsync(book.Id.ToString());
            Assert.True(deletionResult);
        }

        [Fact]
        public async void HashSetUpdateExistingValues()
        {
            var book = new Book()
            {
                Id = 4,
                PublishedOn = DateTime.Now,
                Authors = new List<Author>()
                {
                    new Author()
                    {
                        Firstname = "John",
                        Lastname = "Smith"
                    }
                },
                Rating = null
            };

            await _bookRepository.SaveAsync(book.Id.ToString(), book);

            // update hashset values
            var updatedAuthor = new Author()
            {
                Firstname = "Jonathan",
                Lastname = "Smith"
            };
            var updatedPublishedOn = new DateTime(1994, 8, 3, 20, 30, 0);
            var updatedRating = 10;
            await _bookRepository.HashSetAsync(book.Id.ToString(), new StronglyTypedDictionary<Book>(new StronglyTypedRedisNewtonsoftSerializer())
            {
                { b => b.Rating, updatedRating },
                { b => b.Authors, new List<Author>()
                    {
                        updatedAuthor
                    }
                },
                { b => b.PublishedOn, updatedPublishedOn }
            });

            var updatedBook = await _bookRepository.HashGetAsync(book.Id.ToString(),
                b => b.Id,
                b => b.PublishedOn,
                b => b.Authors,
                b => b.Rating
            );



            Assert.NotNull(updatedBook);

            Assert.True(updatedBook.HasValue(b => b.Rating));
            Assert.True(updatedBook.HasValue(b => b.Id));
            Assert.True(updatedBook.HasValue(b => b.PublishedOn));
            Assert.True(updatedBook.HasValue(b => b.Authors));

            Assert.Equal(updatedPublishedOn.ToShortDateString(), updatedBook.Get<DateTime>(b => b.PublishedOn).ToShortDateString());
            Assert.Equal(updatedPublishedOn.ToShortTimeString(), updatedBook.Get<DateTime>(b => b.PublishedOn).ToShortTimeString());
            Assert.Single(updatedBook.Get<IEnumerable<Author>>(b => b.Authors));
            Assert.Equal(updatedAuthor.Firstname, updatedBook.Get<IEnumerable<Author>>(b => b.Authors).FirstOrDefault()?.Firstname);
            Assert.Equal(updatedAuthor.Lastname, updatedBook.Get<IEnumerable<Author>>(b => b.Authors).FirstOrDefault()?.Lastname);

            Assert.Equal(updatedRating, updatedBook.Get<int?>(b => b.Rating));

            var deletionResult = await _bookRepository.DeleteAsync(book.Id.ToString());
            Assert.True(deletionResult);
        }
    }
}
