using System.Threading.Tasks;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;

namespace ParserFacebookShops.Services.Abstractions
{
    public interface IShopService
    {
        Task<IResult> GetProducts();
    }
}