using System.Collections.Generic;

namespace ParserFacebookShops.Models
{
    public class Product
    {
        public string Name { get; set; }

        public Price Price { get; set; }

        public string Category { get; set; }

        public IReadOnlyCollection<string> ImagesUrl { get; set; }

        public string Description { get; set; }
    }
}