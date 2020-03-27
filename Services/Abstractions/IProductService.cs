using System.Threading.Tasks;
using AngleSharp.Dom;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;

namespace ParserFacebookShops.Services.Abstractions
{
    public interface IProductService
    {
        Task<IResult<Product>> GetFullProductCardAsync(IElement element);

        string GetName(IElement element);

        IResult<Price> ParsePrice(string htmlPrice);
    }
}