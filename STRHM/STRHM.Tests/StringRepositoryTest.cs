using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using STRHM.Configuration;
using STRHM.Repositories;
using STRHM.Serialization.NewtonsoftJson;
using STRHM.Tests.TestData;
using Xunit;

namespace STRHM.Tests
{
    public class StringRepositoryTest
    {
        private readonly BookRepository _bookRepository;

        public StringRepositoryTest()
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
        public async void SetAndGetWholeObject()
        {
            var book = new Book()
            {
                Id = 1,
                PublishedOn = DateTime.Now,
                Authors = new List<Author>()
                {
                    new Author()
                    {
                        Firstname = "John",
                        Lastname = "Smith"
                    }
                },
                Rating = 9
            };

            await _bookRepository.SaveAsync(book.Id.ToString(), book);
            var savedBook = await _bookRepository.GetAsync(book.Id.ToString());

            Assert.NotNull(savedBook);
            Assert.Equal(book.PublishedOn.ToShortDateString(), savedBook.PublishedOn.ToShortDateString());
            Assert.Equal(book.PublishedOn.ToShortTimeString(), savedBook.PublishedOn.ToShortTimeString());
            Assert.Equal(book.Authors.Count, savedBook.Authors.Count);
            Assert.Equal(book.Authors.FirstOrDefault()?.Firstname, savedBook.Authors.FirstOrDefault()?.Firstname);
            Assert.Equal(book.Authors.FirstOrDefault()?.Lastname, savedBook.Authors.FirstOrDefault()?.Lastname);
            Assert.Equal(book.Rating, savedBook.Rating);

            var deletionResult = await _bookRepository.DeleteAsync(book.Id.ToString());
            Assert.True(deletionResult);
        }

        [Fact]
        public async void SetAndGetWholeObjectContainingNullValues()
        {
            var book = new Book()
            {
                Id = 2,
                PublishedOn = DateTime.Now,
                Authors = null,
                Rating = null
            };

            await _bookRepository.SaveAsync(book.Id.ToString(), book);
            var savedBook = await _bookRepository.GetAsync(book.Id.ToString());

            Assert.NotNull(savedBook);
            Assert.Equal(book.PublishedOn.ToShortDateString(), savedBook.PublishedOn.ToShortDateString());
            Assert.Equal(book.PublishedOn.ToShortTimeString(), savedBook.PublishedOn.ToShortTimeString());
            Assert.Null(savedBook.Authors);
            Assert.Null(savedBook.Rating);

            var deletionResult = await _bookRepository.DeleteAsync(book.Id.ToString());
            Assert.True(deletionResult);
        }
    }
}
