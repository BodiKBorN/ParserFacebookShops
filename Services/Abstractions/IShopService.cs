using ParserFacebookShops.Models;
using ParserFacebookShops.Models.Abstractions.Generics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParserFacebookShops.Services.Abstractions
{
    public interface IShopService
    {
        Task<IResult<List<Product>>> GetProductsAsync(string shopId);
    }
}