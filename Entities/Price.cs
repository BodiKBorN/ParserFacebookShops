using ParserFacebookShops.Entities.Abstractions;

namespace ParserFacebookShops.Entities
{
    public class Price : IEntity<int>
    {
        public int Id { get; set; }

        public string Total { get; set; }

        public double? Cost { get; set; }

        public string Currency { get; set; }
        
    }
}