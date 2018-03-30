using System;
using System.Collections.Generic;

namespace STRHM.Console.Models
{
    class BookModel
    {
        public string SerialNumber { get; set; }

        public string Title { get; set; }

        public DateTime PublishedOn { get; set; }

        public bool IsFamilyFriendly { get; set; }

        public int Rating { get; set; }

        //public AuthorModel Author { get; set; }

        //public List<AwardModel> Awards { get; set; }
    }
}
