using System.Collections.Generic;
using System.Threading.Tasks;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;

namespace ParserFacebookShops.Services.Abstractions
{
    public interface IShopService
    {
        Task<IResult<List<Product>>> GetProductsAsync(string shopId);
    }
}