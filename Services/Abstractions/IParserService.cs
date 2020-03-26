using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParserFacebookShops.Services.Abstractions
{
    public interface IParserService
    {
        Task<IResult<List<Product>>> ParseShopAsync(string shopId, bool authentication = false);
    }
}