using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;

namespace ParserFacebookShops.Services.Abstractions
{
    public interface IParser
    {
        Task<IResult<IDocument>> OpenPage(string url);

        Task<IResult> SetAuthentication(string shopId);
    }
}
