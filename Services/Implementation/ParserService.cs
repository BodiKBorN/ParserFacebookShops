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
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using PuppeteerSharp;

namespace ParserFacebookShops.Services.Implementation
{
    public class ParserService : IParserService
    {
        private readonly IProductService _productService;
        private readonly IShopService _shopService;
        public ParserService()
        {
            _shopService = new ShopService();
            _productService = new ProductService();
        }

        public async Task<List<Product>> GetShop(string shopId)
        {
            try
            {

                //Implementation authentication with AngleSharp
                //WebContext.Context.OpenAsync("https://www.facebook.com").Wait();
                //(WebContext.Context.Active.QuerySelector<IHtmlInputElement>("input#email")).Value = "bodik_kz@ukr.net";
                //(WebContext.Context.Active.QuerySelector<IHtmlInputElement>("input#pass")).Value = "201001chepa";
                //await (WebContext.Context.Active.QuerySelector("#loginbutton").Children.FirstOrDefault() as IHtmlInputElement)
                //    .SubmitAsync();

                if (!shopId.EndsWith("/"))
                    shopId += "/";

                shopId += "shop/";

                using var document = await WebContext.Context.OpenAsync(shopId);

                await document.WaitForReadyAsync();

                var selectorResult = document.QuerySelectorAll("#content_container table > tbody > tr td");

                document.Close();

                var querySelector = selectorResult.Length != 0
                    ? selectorResult
                    : (await Program.GetAllProductPageAsync(shopId)).Data;

                if (querySelector.Length == 0)
                    return null;

                var productModels = querySelector
                    .Where(x => x.QuerySelector("div > div > div > a > strong")?.InnerHtml != null)
                    .Select(x =>
                    {
                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
                        var product = new Product
                        {
                            Name = x.QuerySelector("div > div > div > a > strong")?.InnerHtml,
                        };

                        var price = _productService.GetPrice(x.QuerySelector("div > div > div > div span")?.InnerHtml ??
                                             x.QuerySelector("div > div > div > div")?.InnerHtml);

                        if (price.Success)
                            product.Price = price.Data;

                        var pastPrice = _productService.GetPrice(x.QuerySelector("div > div > div > div span:nth-child(2)")
                            ?.InnerHtml);

                        if (pastPrice.Success)
                            product.PastPrice = pastPrice.Data;

                        var htmlAnchorElement = (IHtmlAnchorElement)x.QuerySelector("div > div > div > a");

                        var image = x.QuerySelector("div > div > a img");

                        if (image != null)
                            product.Image = ((IHtmlImageElement)image).Source;

                        return product;
                    })
                    .ToList();

                return productModels;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public void Parse()
        {
            throw new System.NotImplementedException();
        }

        public async Task<IResult<IHtmlCollection<IElement>>> GetShopWithJs(string address)
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

                var waitForSelectorResult = await page.WaitForSelectorAsync("#u_0_4 > ul > li:nth-child(5) > div > div.clearfix._2pi9._2pin._2pic > div.rfloat._ohf > a");

                var href = await (await waitForSelectorResult.GetPropertyAsync("href")).JsonValueAsync<string>()
                           ?? (await page.EvaluateExpressionAsync("(function() {let node = document.querySelector('#u_0_4 > ul > li:nth-child(5) > div > div.clearfix._2pi9._2pin._2pic > div.rfloat._ohf > a'); return !!node ? node.href : null})()")).ToString(); ;

                var s1 = (await waitForSelectorResult.GetPropertyAsync("href")).ToString();
                var s = (await page.EvaluateExpressionAsync("(function() {let node = document.querySelector('#u_0_4 > ul > li:nth-child(5) > div > div.clearfix._2pi9._2pin._2pic > div.rfloat._ohf > a'); return !!node ? node.href : null})()")).ToString();
                await browser.CloseAsync();

                using var document = await WebContext.Context.OpenAsync(href);

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