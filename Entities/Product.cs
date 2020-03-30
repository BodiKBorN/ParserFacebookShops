using System.Collections.Generic;
using ParserFacebookShops.Entities.Abstractions;

namespace ParserFacebookShops.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }

        public Price Price { get; set; }

        public Price PastPrice { get; set; }

        public string Category { get; set; }

        public IReadOnlyCollection<string> ImageUrl { get; set; }

        public string Description { get; set; }
    }
}