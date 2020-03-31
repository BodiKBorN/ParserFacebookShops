using ParserFacebookShops.Entities;
using ParserFacebookShops.Models.Abstractions.Generics;
using ParserFacebookShops.Models.Implementation.Generics;
using ParserFacebookShops.Services.Abstractions;
using PuppeteerSharp;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParserFacebookShops.Services.Implementation
{
    public class PuppeteerSharpParser : IDisposable
    {
        private Browser Context { get; }

        private readonly IProductService _productService;

        public PuppeteerSharpParser()
        {
            Context = Task.Run(GetBrowserAsync).Result;
            _productService = new ProductService();
        }

        public async Task<Page> OpenPageAsync(string address)
        {
            try
            {
                var page = Context.NewPageAsync().Result;

                await page.GoToAsync(address);

                return page;
            }
            catch(Exception e)
            {
                throw;
            }
        }

        public async Task<IResult<string>> GetHrefAllProductsPageAsync(string address)
        {
            try
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " in PuppeteerSharp GetHrefAllProductsPageAsync");

                using var page = await OpenPageAsync(address);

                await page
                    .EvaluateExpressionAsync("scrollTo(0, document.querySelector('body').scrollHeight)");

                var waitForSelectorResult = await page
                    .WaitForSelectorAsync("#u_0_4 > ul > li:last-child > div > div > div > a");

                if (waitForSelectorResult == null)
                    return Result<string>.CreateFailed("SELECT_ELEMENTS_ERROR");

                var href = await (await waitForSelectorResult.GetPropertyAsync("href")).JsonValueAsync<string>()
                           ?? await page.EvaluateExpressionAsync<string>("(function() {let node = document.querySelector('#u_0_4 > ul > li:last-child > div > div > div > a'); return !!node ? node.href : null})()");

                await page.CloseAsync();

                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " out PuppeteerSharp GetHrefAllProductsPageAsync");

                return href == null
                    ? Result<string>.CreateFailed("PAGE_HREF_NOT_FOUND")
                    : Result<string>.CreateSuccess(href);
            }
            catch (Exception e)
            {
                return Result<string>.CreateFailed("ERROR_GET_ALL_PRODUCT_PAGE");
            }
        }

        public async Task<IResult<Product>> GetProductCard(string cardUrl)
        {
            try
            {
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " in PuppeteerSharp GetProductCard");
                using var page = await OpenPageAsync(cardUrl);

                //await page.SetJavaScriptEnabledAsync(false);

                var contentAsync = await page.GetContentAsync();

                var name = await page
                    .EvaluateExpressionAsync<string>("document.querySelector('#u_0_y > div').innerText");

                var htmlPrice = await page
                    .EvaluateExpressionAsync<string>("document.querySelector('#u_0_y > div > div > div > div > div > div').innerHTML");

                var category = await page
                    .EvaluateExpressionAsync<string>("(function() { let node = document.querySelector('#u_0_y > div > div > span > div > div > div > a > span'); return !!node ? node.innerText : null})()");

                var showMoreButton = await page
                    .QuerySelectorAsync("#u_0_y > ul > li > div > div");

                if (showMoreButton != null)
                    await showMoreButton.ClickAsync();

                var description = await page
                    .EvaluateExpressionAsync<string>("document.querySelector('#u_0_y > ul > li > div._1xwp').innerText");

                var imageUrl = (await page.EvaluateExpressionAsync("Array.from(document.querySelectorAll('div > div > div > div > div > div > div > div > div > div > div > div._6e_ > div')).map(x=>x.style.backgroundImage)"))
                    .Values<string>()
                    .Select(htmlImageUrl => _productService.ParseImageUrl(htmlImageUrl))
                    .ToList();

                var price = _productService.GetPrice(htmlPrice);

                if (name == null && htmlPrice == null && !price.Success)
                    return Result<Product>.CreateFailed();

                var product = new Product()
                {
                    Name = name,
                    Price = price.Data,
                    Description = description,
                    Category = category,
                    ImageUrl = imageUrl
                };

                await page.CloseAsync();
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " out PuppeteerSharp GetProductCard");
                return Result<Product>.CreateSuccess(product);
            }
            catch(Exception e)
            {
                return Result<Product>.CreateFailed("GETTING_PRODUCT_CARD_ERROR");
            }
        }

        private async Task<Browser> GetBrowserAsync()
        {
            try
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);

                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                });

                //Authentication 
                var page = await browser.NewPageAsync();

                await page.GoToAsync("https://www.facebook.com/");
                await page.EvaluateExpressionAsync("document.querySelector('#email').value = 'bodik_kz@ukr.net'");
                await page.EvaluateExpressionAsync("document.querySelector('#pass').value = '201001chepa'");
                await page.EvaluateExpressionAsync("document.querySelector('#loginbutton').click()");
                await page.WaitForNavigationAsync();

                return browser;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}