using System.Collections.Generic;
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
        IResult<Price> GetPrice(string htmlPrice);

        string ParseImageUrl(string imageInnerText);
    }
}