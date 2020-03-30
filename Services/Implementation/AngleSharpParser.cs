using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;

namespace ParserFacebookShops.Services.Implementation
{
    public class AngleSharpParser
    {
        public IBrowsingContext Context { get; }

        public bool HasAuthentication { get; set; }

        private readonly PuppeteerSharpParser _puppeteerSharpParser;
        private readonly IProductService _productService;

        public AngleSharpParser()
        {
            Context = ParserContext.AngleSharpContext;
            _puppeteerSharpParser = new PuppeteerSharpParser();
            _productService = new ProductService();
        }

        public async Task<IResult<IDocument>> OpenPageAsync(string url)
        {
            var document = await Context.OpenAsync(url);

            await document.WaitForReadyAsync();

            return Result<IDocument>.CreateSuccess(document);
        }

        public async Task<IHtmlCollection<IElement>> GetElementsFromShopPageAsync(string shopId)
        {
             var document = (await OpenPageAsync(shopId));

             //if (!document.Success)
             //    return Result<List<Product>>.CreateFailed();

             var selectorResult = document.Data.QuerySelectorAll("#content_container table > tbody > tr td");

             document.Data.Close();

             return selectorResult;
        }

        public async Task<IHtmlCollection<IElement>> GetElementsFromAllProductPageAsync(string shopUrl)
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " in AngleSharp GetElementsFromAllProductPageAsync");

            var href = await _puppeteerSharpParser.GetHrefAllProductsPageAsync(shopUrl);

            if (!href.Success)
                return null;

            using var document = await ParserContext.AngleSharpContext.OpenAsync(href.Data);

            var result = document.QuerySelectorAll("tbody > tr td");

            document.Close();
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " out AngleSharp GetElementsFromAllProductPageAsync");
            return result;
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

                        //var price = _productService.ParsePrice(x.QuerySelector("div > div > div > div span")?.InnerHtml
                        //                                       ?? x.QuerySelector("div > div > div > div")?.InnerHtml);

                        //if (price.Success)
                        //    product.Price = price.Data;

                        //var pastPrice = _productService.ParsePrice(x.QuerySelector("div > div > div > div span:nth-child(2)")?.InnerHtml);

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

        public IResult<string> GetProductCardHref(IElement element)
        {
            try
            {
                if (element == null)
                    Result<Product>.CreateFailed();

                var querySelector = element.QuerySelector("div > div > div > a");

                if (!(querySelector is IHtmlAnchorElement))
                    return Result<string>.CreateFailed();

                var cardUrl = (querySelector as IHtmlAnchorElement).Href;

                return Result<string>.CreateSuccess(cardUrl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public string GetName(IElement element) =>
            element.QuerySelector("div > div > div > a > strong")?.InnerHtml;

        public async Task SetAuthentication(string shopId)
        {
            try
            {
                //Implementation authentication with AngleSharp
                Context.OpenAsync(shopId).Wait();
                (Context.Active.QuerySelector<IHtmlInputElement>("input#email")).Value = "bodik_kz@ukr.net";
                (Context.Active.QuerySelector<IHtmlInputElement>("input#pass")).Value = "201001chepa";

                await (Context.Active.QuerySelector("#loginbutton").Children.FirstOrDefault() as IHtmlInputElement)
                    .SubmitAsync();

                HasAuthentication = true;

                //return Result.CreateSuccess();
            }
            catch (Exception e)
            {
               //return Result.CreateFailed("AUTHENTICATION_ERROR");
            }
        }
    }
}