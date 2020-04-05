using ParserFacebookShops.Entities.Abstractions;

namespace ParserFacebookShops.Entities
{
    public class Price
    {
        public string Total { get; set; }

        public decimal? Cost { get; set; }

        public string Currency { get; set; }
        
    }
}