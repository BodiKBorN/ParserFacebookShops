using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParserFacebookShops.Models.Implementation;

namespace ParserFacebookShops.Services.Implementation
{
    public class ShopService : IShopService, IParseble
    {
        private readonly IProductService _productService;
        private readonly IParser _parser;

        public ShopService()
        {
            _productService = new ProductService();
            _parser = GetParser().Data;
        }

        public async Task<IResult<List<Product>>> GetProductsAsync(string shopId)
        {
            try
            {
                if (!shopId.EndsWith("/"))
                    shopId += "/";

                shopId += "shop/";

                var document = await _parser.OpenPage(shopId);

                if (!document.Success)
                    return Result<List<Product>>.CreateFailed();

                var selectorResult = document.Data.QuerySelectorAll("#content_container table > tbody > tr td");

                document.Data.Close();

                var productElements = selectorResult.Length != 0
                    ? selectorResult
                    : (await GetElementsFromAllProductsPageAsync(shopId)).Data;

                if (productElements.Length == 0)
                    return Result<List<Product>>.CreateFailed("ELEMENTS_NOT_FOUND");

                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " 2");

                var productModels = productElements
                    .Where(x => _productService.GetName(x) != null)
                    .Select(async x =>
                    {
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " 3");

                        var product = new Product
                        {
                            Name = _productService.GetName(x)
                        };

                        var price = _productService.ParsePrice(x.QuerySelector("div > div > div > div span")?.InnerHtml
                                                             ?? x.QuerySelector("div > div > div > div")?.InnerHtml);

                        if (price.Success)
                            product.Price = price.Data;

                        var pastPrice = _productService.ParsePrice(x.QuerySelector("div > div > div > div span:nth-child(2)")?.InnerHtml);

                        if (pastPrice.Success)
                            product.PastPrice = pastPrice.Data;

                        var fullProductCardAsync = await _productService.GetFullProductCardAsync(x);

                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " 3 ----past");
                        var image = x.QuerySelector("div > div > a img");

                        if (image != null)
                            product.Image = ((IHtmlImageElement)image).Source;

                        return product;
                    })
                    .ToList();

                var whenAll = await Task.WhenAll(productModels);
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " 4");

                return Result<List<Product>>.CreateSuccess(whenAll.ToList());
            }
            catch (Exception e)
            {
                return Result<List<Product>>.CreateFailed("ERROR_GET_PRODUCTS");
            }
        }

        private async Task<IResult<IHtmlCollection<IElement>>> GetElementsFromAllProductsPageAsync(string address)
        {
            try
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });

                var page = await browser.NewPageAsync();

                await page.GoToAsync(address);

                //Authentication 
                await page.EvaluateExpressionAsync("document.querySelector('#email').value = 'bodik_kz@ukr.net'");
                await page.EvaluateExpressionAsync("document.querySelector('#pass').value = '201001chepa'");
                await page.EvaluateExpressionAsync("document.querySelector('#loginbutton').click()");
                await page.WaitForNavigationAsync();

                await page.EvaluateExpressionAsync("scrollTo(0, document.querySelector('body').scrollHeight)");

                var waitForSelectorResult = await page.WaitForSelectorAsync("#u_0_4 > ul > li:last-child > div > div > div > a");

                if (waitForSelectorResult == null)
                    return Result<IHtmlCollection<IElement>>.CreateFailed("SELECT_ELEMENTS_ERROR");

                var href = await (await waitForSelectorResult.GetPropertyAsync("href")).JsonValueAsync<string>()
                           ?? (await page.EvaluateExpressionAsync("(function() {let node = document.querySelector('#u_0_4 > ul > li:last-child > div > div > div > a'); return !!node ? node.href : null})()")).ToString();

                await browser.CloseAsync();

                if (href == null)
                    return Result<IHtmlCollection<IElement>>.CreateFailed("PAGE_HREF_NOT_FOUND");

                using var document = await ParserContext.AngleSharpContext.OpenAsync(href);

                var result = document.QuerySelectorAll("tbody > tr td");

                document.Close();

                return result == null
                    ? Result<IHtmlCollection<IElement>>.CreateFailed("ELEMENTS_NOT_FOUND")
                    : Result<IHtmlCollection<IElement>>.CreateSuccess(result);
            }
            catch (Exception e)
            {
                return Result<IHtmlCollection<IElement>>.CreateFailed("ERROR_GET_ALL_PRODUCT_PAGE");
            }
        }

        public IResult<IParser> GetParser()
        {
            IParser parser = new AngleSharpParser();

            return Result<IParser>.CreateSuccess(parser);
        }
    }
}