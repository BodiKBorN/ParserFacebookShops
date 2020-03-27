using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;

namespace ParserFacebookShops.Services.Implementation
{
    public class AngleSharpParser : IParser
    {
        public IBrowsingContext Context { get; }

        public AngleSharpParser()
        {
            Context = BrowsingContext.New(Configuration.Default.WithDefaultLoader().WithDefaultCookies());
        }

        public async Task<IResult<IDocument>> OpenPage(string url)
        {
            using var document = await ParserContext.AngleSharpContext.OpenAsync(url);

            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " 1");
            await document.WaitForReadyAsync();

            return Result<IDocument>.CreateSuccess(document);
        }

        public async Task<IResult> SetAuthentication(string shopId)
        {
            try
            {
                //Implementation authentication with AngleSharp
                Context.OpenAsync(shopId).Wait();
                (Context.Active.QuerySelector<IHtmlInputElement>("input#email")).Value = "bodik_kz@ukr.net";
                (Context.Active.QuerySelector<IHtmlInputElement>("input#pass")).Value = "201001chepa";

                await (Context.Active.QuerySelector("#loginbutton").Children.FirstOrDefault() as IHtmlInputElement)
                    .SubmitAsync();

                return Result.CreateSuccess();
            }
            catch (Exception e)
            {
                return Result.CreateFailed("AUTHENTICATION_ERROR");
            }
        }
    }
}