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

namespace ParserFacebookShops.Services.Implementation
{
    public class ShopService : IShopService
    {
        private readonly IProductService _productService;

        public ShopService()
        {
            _productService = new ProductService();
        }

        public async Task<IResult<List<Product>>> GetProductsAsync(string shopId)
        {
            try
            {
                if (!shopId.EndsWith("/"))
                    shopId += "/";

                shopId += "shop/";

                using var document = await ParserContext.AngleSharpContext.OpenAsync(shopId);

                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " 1");

                await document.WaitForReadyAsync();

                var selectorResult = document.QuerySelectorAll("#content_container table > tbody > tr td");

                document.Close();

                var productElements = selectorResult.Length != 0
                    ? selectorResult
                    : (await GetElementsFromAllProductsPageAsync(shopId)).Data;

                if (productElements.Length == 0)
                    return Result<List<Product>>.CreateFailed("ELEMENTS_NOT_FOUND"); ;

                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " 2");

                var productModels = productElements
                    .Where(x => _productService.GetName(x) != null)
                    .Select(x =>
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

                        var htmlAnchorElement = (IHtmlAnchorElement)x.QuerySelector("div > div > div > a");

                        var image = x.QuerySelector("div > div > a img");

                        if (image != null)
                            product.Image = ((IHtmlImageElement)image).Source;

                        return product;
                    })
                    .ToList();

                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " 4");

                return Result<List<Product>>.CreateSuccess(productModels);
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
    }
}