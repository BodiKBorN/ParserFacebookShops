using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation;
using ParserFacebookShops.Models.Implementation.Generics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IHtmlCollection<IElement>> GetElementsFromShopPageAsync(string shopId)
        {
            try
            {
                await SetAuthenticationForFacebookAsync();

                using var document = (await OpenPageAsync(shopId));

                var selectorResult = document.QuerySelectorAll("#content_container table > tbody > tr td");

                document.Close();

                return selectorResult;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IHtmlCollection<IElement>> GetElementsFromAllProductPageAsync(string shopUrl)
        {
            try
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " in AngleSharp GetElementsFromAllProductPageAsync");

                var href = await _puppeteerSharpParser.GetHrefAllProductsPageAsync(shopUrl);

                if (!href.Success)
                    return null;

                using var document = await Context.OpenAsync(href.Data);

                var result = document.QuerySelectorAll("tbody > tr td");

                document.Close();
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " out AngleSharp GetElementsFromAllProductPageAsync");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public List<Task<Product>> GetProducts(IHtmlCollection<IElement> productElements)
        {
            try
            {
                return productElements
                    .Where(x => GetName(x) != null)
                    .Select(async x =>
                    {
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " in select AngleSharp GetProducts");

                        var productCardHref = GetProductCardHref(x);

                        if (!productCardHref.Success)
                            return null;

                        var productCard = await _puppeteerSharpParser.GetProductCard(productCardHref.Data);

                        //var product = new Product();

                        //product.Name = GetName(x);

                        //var price = _productService.GetPrice(x.QuerySelector("div > div > div > div span")?.InnerHtml
                        //                                       ?? x.QuerySelector("div > div > div > div")?.InnerHtml);

                        //if (price.Success)
                        //    product.Price = price.Data;

                        //var pastPrice = _productService.GetPrice(x.QuerySelector("div > div > div > div span:nth-child(2)")?.InnerHtml);

                        //if (pastPrice.Success)
                        //    product.PastPrice = pastPrice.Data;

                        //Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " 3 ----past");

                        //var image = x.QuerySelector("div > div > a img");

                        ////if (image != null)
                        ////    product.ImageUrl = (image as IHtmlImageElement)?.Source;
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " out select AngleSharp GetProducts");
                        return productCard.Data;
                    })
                    .ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IResult> SetAuthenticationForFacebookAsync()
        {
            try
            {
                //Implementation authentication with AngleSharp
                Context.OpenAsync("https://www.facebook.com").Wait();
                (Context.Active.QuerySelector<IHtmlInputElement>("input#email")).Value = "bodik_kz@ukr.net";
                (Context.Active.QuerySelector<IHtmlInputElement>("input#pass")).Value = "201001chepa";

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