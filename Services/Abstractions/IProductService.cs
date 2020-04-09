using ParserFacebookShops.Models;
using ParserFacebookShops.Models.Abstractions.Generics;

namespace ParserFacebookShops.Services.Abstractions
{
    public interface IProductService
    {
        IResult<Price> GetPrice(string htmlPrice, string pageLanguage);

        IResult<string> ParseImageUrl(string imageInnerText);
    }
}