using System.Threading.Tasks;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;

namespace ParserFacebookShops.Services.Abstractions
{
    public interface IProductService
    {
        Task<IResult> GetFullProductCard();

        IResult<string> GetName();

        IResult<Price> GetPrice(string htmlPrice);

        Task<IResult> GetImage();

        IResult GetDescription();
    }
}