using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation;
using ParserFacebookShops.Models.Implementation.Generics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ParserFacebookShops.Services.Implementation
{
    public class AngleSharpParser : IDisposable
    {
        public IBrowsingContext Context { get; }

        private readonly PuppeteerSharpParser _puppeteerSharpParser;

        public AngleSharpParser()
        {
            Context = BrowsingContext.New(Configuration.Default.WithDefaultLoader().WithDefaultCookies());
            _puppeteerSharpParser = new PuppeteerSharpParser();
        }

        public async Task<IDocument> OpenPageAsync(string url)
        {
            try
            {
                var document = await Context.OpenAsync(url);

                await document.WaitForReadyAsync();
                
                return document;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<IResult<IHtmlCollection<IElement>>> GetElementsFromShopPageAsync(string shopId)
        {
            try
            {
                await SetAuthenticationForFacebookAsync();

                using var document = (await OpenPageAsync(shopId));

                var selectorResult = document.QuerySelectorAll("#content_container table > tbody > tr td");

                document.Close();

                return Result<IHtmlCollection<IElement>>.CreateSuccess(selectorResult);
            }
            catch
            {
                return Result<IHtmlCollection<IElement>>.CreateFailed("GETTING_ELEMENTS_FROM_SHOP_PAGE_ERROR");
            }
        }

        public async Task<IResult<IHtmlCollection<IElement>>> GetElementsFromAllProductPageAsync(string shopUrl)
        {
            try
            {
                var hrefAllProductsPage = await _puppeteerSharpParser.GetHrefAllProductsPageAsync(shopUrl);

                if (!hrefAllProductsPage.Success)
                    return Result<IHtmlCollection<IElement>>.CreateFailed(hrefAllProductsPage.Message);

                using var document = await Context.OpenAsync(hrefAllProductsPage.Data);

                var result = document.QuerySelectorAll("tbody > tr td");

                document.Close();

                return Result<IHtmlCollection<IElement>>.CreateSuccess(result);
            }
            catch
            {
                return Result<IHtmlCollection<IElement>>.CreateFailed("GETTING_ELEMENTS_FROM_ALL_PRODUCT_PAGE_ERROR");
            }
        }

        public IResult<Task<Product>[]> GetProducts(IHtmlCollection<IElement> productElements)
        {
            try
            {
                var result = productElements
                    .Where(x => GetName(x) != null)
                    .Select(async x =>
                    {
                        var productCardHref = GetProductCardHref(x);

                        if (!productCardHref.Success)
                            return null;

                        var productCard = await _puppeteerSharpParser.GetProductCard(productCardHref.Data);

                        if (!productCard.Success)
                            return null;

                        return productCard.Data;
                    })
                    .ToArray();

                return Result<Task<Product>[]>.CreateSuccess(result);
            }
            catch
            {
                return Result<Task<Product>[]>.CreateFailed("GETTING_PRODUCTS_ERROR");
            }
        }

        public string GetName(IElement element) =>
            element.QuerySelector("div > div > div > a > strong")?.InnerHtml;

        public IResult<string> GetProductCardHref(IElement productElement)
        {
            try
            {
                if (productElement == null)
                    return Result<string>.CreateFailed("NOT_CORRECT_DATA");

                var querySelector = productElement.QuerySelector("div > div > div > a");

                if (!(querySelector is IHtmlAnchorElement))
                    return Result<string>.CreateFailed("ELEMENT_CAST_ERROR");

                var cardUrl = (querySelector as IHtmlAnchorElement).Href;

                return Result<string>.CreateSuccess(cardUrl);
            }
            catch
            {
                return Result<string>.CreateFailed("GETTING_CARD_HREF_ERROR");
            }
        }

        public async Task<IResult> SetAuthenticationForFacebookAsync()
        {
            try
            {
                //Implementation authentication with AngleSharp
                Context.OpenAsync("https://www.facebook.com").Wait();
                (Context.Active.QuerySelector<IHtmlInputElement>("input#email")).Value = AuthenticationData.Login;
                (Context.Active.QuerySelector<IHtmlInputElement>("input#pass")).Value = AuthenticationData.Password;

                await (Context.Active.QuerySelector("#loginbutton").Children.FirstOrDefault() as IHtmlInputElement)
                    .SubmitAsync();

                return Result.CreateSuccess();
            }
            catch
            {
                return Result.CreateFailed("AUTHENTICATION_ERROR");
            }
        }

        public void Dispose()
        {
            _puppeteerSharpParser?.Dispose();
        }
    }
}