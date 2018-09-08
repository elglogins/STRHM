using System;
using System.Collections.Generic;
using System.Text;
using STRHM.Attributes;

namespace STRHM.Tests.TestData
{
    public class Book
    {
        public int Id { get; set; }

        public int? Rating { get; set; }

        public DateTime PublishedOn { get; set; }

        [SerializableRedisProperty]
        public List<Author> Authors { get; set; }
    }
}
