using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParserFacebookShops.Services.Abstractions
{
    public interface IParseble
    {
        IResult<IParser> GetParser();
    }
}