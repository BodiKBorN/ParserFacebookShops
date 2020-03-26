using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;

namespace ParserFacebookShops.Services.Abstractions
{
    public interface IParserService
    {
        Task<List<Product>> GetShop(string shopId);

        public void Parse();

        Task<IResult<IHtmlCollection<IElement>>> GetShopWithJs(string address);
    }
}