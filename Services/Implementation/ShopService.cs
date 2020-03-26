using System.Threading.Tasks;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Services.Abstractions;

namespace ParserFacebookShops.Services.Implementation
{
    public class ShopService : IShopService
    {
        public Task<IResult> GetProducts()
        {
            throw new System.NotImplementedException();
        }
    }
}