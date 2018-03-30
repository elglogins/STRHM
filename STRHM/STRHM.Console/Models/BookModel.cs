using System;
using System.Collections.Generic;
using STRHM.Attributes;

namespace STRHM.Console.Models
{
    class BookModel
    {
        public string SerialNumber { get; set; }

        public string Title { get; set; }

        public DateTime PublishedOn { get; set; }

        public bool Featured { get; set; }

        public int Rating { get; set; }

        [SerializableRedisProperty]
        public AuthorModel Author { get; set; }

        [SerializableRedisProperty]
        public List<AwardModel> Awards { get; set; }
    }
}
