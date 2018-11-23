# Strongly Typed Redis Hashset Model
[Introduction article](https://medium.com/@elgarslogins/tired-of-magical-strings-for-redis-hashes-ea4d4d9e0dc)

## Supported frameworks

 - Version 1.x - .NET 4.6.1 and NETStandard1.5
 - Version 2.x - .NET 4.6.1 and .NETStandard2.0
 
## Example of usage

Example entity will be a Book class, that has couple of basic fields.

    public class Book
    {
        public int Id { get; set; }

        public int? Rating { get; set; }

        public DateTime PublishedOn { get; set; }

        [SerializableRedisProperty]
        public List<Author> Authors { get; set; }
    }

That’s right, you noticed a custom attribute. This attribute is responsible for particular property being serialized when it is being set into Redis hash as a value.


Book repository inherits from BaseRedisHashSetRepository, as it exposes most common needed functionality and database connection, in case if you want to extend functionality in Book repository itself.

    public class BookRepository : BaseRedisHashSetRepository<Book>
    {
        public BookRepository(IRedisConnection redisConnection, IStronglyTypedRedisSerializer serializer, RedisHashSetOptions configurationOptions) 
            : base(redisConnection, serializer, configurationOptions)
        {
        }
    }
**Constructor parameters:**

 - **IRedisConnection** - Is responsible for creating and serving connection to Redis. It can be your own implementation for specific use case, but by default you can use RedisConnection implementation, which expects your configuration options in its constructor.
 - **IStronglyTypedRedisSerializer** - Used for serializing and deserializing values into and from Redis. It is up to you which library to use - Jil, Newtonsoft or any other. You can use [STRHM.Serialization.NewtonsoftJson](https://www.nuget.org/packages/STRHM.Serialization.NewtonsoftJson/) for Newtonsoft.Json.
 - **RedisHashSetOptions** - Configuration object for your repository, what namespaces to be used, which database index and command flags.
 
**Examples of getting values:**

    // Check if value is set on property
    if (updatedBook.HasValue(b => b.Rating))
    // Get values
    updatedBook.Get<IEnumerable<Author>>(b => b.Authors)
    updatedBook.Get<int?>(b => b.Rating)

or
   

    var book = await _bookRepository.GetAsync(book.Id.ToString());


**Examples of setting values:**

    await _bookRepository.SaveAsync(book.Id.ToString(), book);
or

    await _bookRepository.HashSetAsync(book.Id.ToString(), new StronglyTypedDictionary<Book>(new StronglyTypedRedisNewtonsoftSerializer())  
    {  
          { b => b.Rating, 10 },  
          { b => b.Authors, new List<Author>()},  
          { b => b.PublishedOn, DateTime.Now }  
     });

